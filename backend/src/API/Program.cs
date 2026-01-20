using System.Text;
using Auth.Application;
using Auth.Infrastructure;
using LibraryApp.API.Middlewares;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Microsoft.IdentityModel.Tokens;
using Auth.Infrastructure.Data;
using Scalar.AspNetCore;
using LibraryApp.API.Endpoints.Auth;

namespace LibraryApp.API;

/// <summary>
/// Main application entry point and configuration.
/// </summary>
public class Program
{
    private static void Main(string[] args)
    {
        var builder = WebApplication.CreateBuilder(args);

        // Add services
        builder.Services
            .AddEndpointsApiExplorer()
            .AddAuthApplicationServices();
        
        // Only register SQL Server infrastructure if not running integration tests
        if (!builder.Environment.IsEnvironment("IntegrationTests"))
        {
            builder.Services
                .AddAuthInfrastructure(
                    builder.Configuration.GetConnectionString("SqlConnectionString")
                    ?? "Server=.;Database=LibraryApp;Trusted_Connection=true;Encrypt=false"
                );
        }
        
        builder.Services
            .AddAuthenticationServices(builder.Configuration);

        builder.Services
            .AddControllers()
            .AddControllersAsServices();

        builder.Services
            .AddExceptionHandler<ValidationExceptionHandler>()
            .AddProblemDetails();

        // JWT Authentication
        builder.Services.AddAuthentication(options =>
        {
            options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
            options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
        })
        .AddJwtBearer(options =>
        {
            options.TokenValidationParameters = new TokenValidationParameters
            {
                ValidateIssuer = true,
                ValidateAudience = true,
                ValidateLifetime = true,
                ValidateIssuerSigningKey = true,
                ValidIssuer = builder.Configuration["Jwt:Issuer"],
                ValidAudience = builder.Configuration["Jwt:Audience"],
                IssuerSigningKey = new SymmetricSecurityKey(
                    Encoding.UTF8.GetBytes(builder.Configuration["Jwt:SecretKey"]
                    ?? throw new InvalidOperationException("No JWT Secret Key configured")))
            };
        });

        // builder.Services.AddHostedService<PolicyInitializationService>();

        // Authorization
        builder.Services.AddAuthorization();

        builder.Services.AddEndpointsApiExplorer();
        builder.Services.AddOpenApi();

        builder.Services.AddHealthChecks()
            .AddCheck("healthy", () => HealthCheckResult.Healthy());

        // Add CORS policy
        builder.Services.AddCors(options =>
        {
            options.AddPolicy("AllowBlazorOrigin", policy =>
            {
                var origins = builder.Configuration.GetSection("BlazorWasmOrigins").Get<string[]>()
                    ?? throw new InvalidOperationException("No Blazor WASM origins configured");

                policy.WithOrigins(origins)
                      .AllowAnyHeader()
                      .AllowAnyMethod()
                      .AllowCredentials();
            });
        });

        // Build application
        var app = builder.Build();

        if (app.Environment.IsDevelopment())
        {
            app.MapOpenApi();
            app.MapScalarApiReference(options =>
            {
                options
                    .WithTitle("LibraryApp API")
                    .WithTheme(ScalarTheme.DeepSpace)
                    .WithDefaultHttpClient(ScalarTarget.CSharp, ScalarClient.HttpClient);
            });

            EnsureDatabaseCreation(app);
        }

        app.UseExceptionHandler();
        // app.UseHttpsRedirection();

        // Register endpoints
        app.AddAuthEndpoints();
        app.AddUserEndpoints();
        app.AddRoleEndpoints();

        // Middleware
        app.UseAuthentication();
        app.UseAuthorization();
        app.UseMiddleware<AuthorizationMiddleware>();

        // Health checks
        app.MapHealthChecks("/health", new HealthCheckOptions
        {
            Predicate = registration => registration.Name == "healthy",
            ResponseWriter = async (context, report) =>
            {
                context.Response.ContentType = "application/json";
                await context.Response.WriteAsJsonAsync(new
                {
                    status = report.Status.ToString(),
                    checks = report.Entries.ToDictionary(
                        x => x.Key,
                        x => new { status = x.Value.Status.ToString() }
                    )
                });
            }
        });

        app.Run();
    }

    private static void EnsureDatabaseCreation(WebApplication app)
    {
        using var scope = app.Services.CreateScope();
        var authDbContext = scope.ServiceProvider.GetRequiredService<AuthDbContext>();
        var retryCount = 0;
        var maxRetries = 3;
        var delay = TimeSpan.FromSeconds(5);

        while (retryCount < maxRetries)
        {
            try
            {
                Console.WriteLine($"Attempting to ensure database is created (Attempt {retryCount + 1}/{maxRetries})");
                Console.WriteLine($"Attempting to run migrations");
                authDbContext.Database.Migrate();
                Console.WriteLine("Database initialization/migration completed successfully");

                // Execute initial data seed script if it exists
                ExecuteInitialDataSeed(authDbContext);

                break;
            }
            catch (Exception ex)
            {
                retryCount++;
                Console.WriteLine($"Error initializing database: {ex.Message}. Retrying in {delay.TotalSeconds} seconds...");

                if (retryCount >= maxRetries)
                {
                    Console.WriteLine("Failed to initialize database after multiple attempts");
                    throw;
                }

                Task.Delay(delay).Wait();
                delay = TimeSpan.FromSeconds(Math.Min(delay.TotalSeconds * 2, 30));
            }
        }
    }

    private static void ExecuteInitialDataSeed(AuthDbContext authDbContext)
    {
        try
        {
            // Check if admin user already exists
            var adminExists = authDbContext.Users.Any(u => u.Username == "admin");
            if (adminExists)
            {
                Console.WriteLine("Initial data seed already applied (admin user exists). Skipping...");
                return;
            }

            Console.WriteLine("Executing initial data seed script...");
            
            // Construct path to seed script
            // The InitialDataSeed.sql is in the same directory as Program.cs
            var currentDir = AppContext.BaseDirectory; // bin/Debug/net10.0
            var projectDir = Path.Combine(currentDir, "..", "..", "..");
            projectDir = Path.GetFullPath(projectDir);
            
            var seedScriptPath = Path.Combine(projectDir, "InitialDataSeed.sql");
            
            Console.WriteLine($"Looking for seed script at: {seedScriptPath}");
            
            if (File.Exists(seedScriptPath))
            {
                var seedScript = File.ReadAllText(seedScriptPath);
                authDbContext.Database.ExecuteSqlRaw(seedScript);
                Console.WriteLine("Initial data seed script executed successfully");
            }
            else
            {
                Console.WriteLine($"Warning: Seed script not found at {seedScriptPath}");
                // List files in project directory for debugging
                var files = Directory.GetFiles(projectDir, "*.*");
                if (files.Length > 0)
                {
                    Console.WriteLine($"Files in {projectDir}: {string.Join(", ", files.Select(f => Path.GetFileName(f)).Take(10))}");
                }
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error executing initial data seed: {ex.Message}");
            // Don't throw - allow app to continue even if seed fails
        }
    }
}
