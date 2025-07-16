using Application;
using FluentValidation;
using Infrastructure;
using Infrastructure.Persistence.Context;
using LibraryApp.API.Middlewares;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Microsoft.AspNetCore.Mvc.Controllers;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Microsoft.OpenApi.Models;

namespace LibraryApp.API;

public class Program
{
    private static void Main(string[] args)
    {
        var builder = WebApplication.CreateBuilder(args);

        // Add services to the container.
        builder.Services
            .AddEndpointsApiExplorer()
            .AddApplicationDependencies()
            .AddInfrastructureDependencies();

        builder.Services.AddControllers(options =>
        {
            options.Filters.Add<ValidationExceptionFilter>();
        });

        builder.Services.AddSwaggerGen(c =>
        {
            c.SwaggerDoc("v1", new OpenApiInfo { Title = "Library App", Version = "v1" });
            c.TagActionsBy(api =>
            {
                if (api.GroupName != null)
                {
                    return [api.GroupName];
                }

                if (api.ActionDescriptor is ControllerActionDescriptor controllerActionDescriptor)
                {
                    return [controllerActionDescriptor.ControllerName];
                }

                throw new InvalidOperationException("Unable to determine tag for endpoint.");
            });

            // var xmlFile = $"{Assembly.GetExecutingAssembly().GetName().Name}.xml";
            // var xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFile);
            // c.IncludeXmlComments(xmlPath);
            c.DocInclusionPredicate((name, api) => true);
        });

        if (builder.Environment.IsDevelopment() || builder.Environment.IsProduction())
        {
            builder.Services.AddDbContext<SqlDataContext>(options =>
            {
                options.UseSqlServer(builder.Configuration.GetConnectionString("SqlConnectionString"),
                    sqlServerOptionsAction: sqlOptions =>
                    {
                        sqlOptions.EnableRetryOnFailure(
                            maxRetryCount: 5,
                            maxRetryDelay: TimeSpan.FromSeconds(30),
                            errorNumbersToAdd: null);
                    });
            });
        }

        builder.Services.AddValidatorsFromAssemblyContaining<Program>();
        builder.Services.Configure<MongoDbConfiguration>(builder.Configuration.GetSection("MongoDbDatabase"));
        builder.Services.AddTransient<NoSqlDataContext>();

        // Liveness check
        builder.Services.AddHealthChecks().AddCheck("healthy", () => HealthCheckResult.Healthy());

        var app = builder.Build();

        // Configure the HTTP request pipeline.
        if (app.Environment.IsDevelopment())
        {
            app.UseDeveloperExceptionPage();
        }

        if (app.Environment.IsDevelopment() || app.Environment.IsDevelopment())
        {
            EnsureDatabaseCreation(app);
        }

        app.UseSwagger();
        app.UseSwaggerUI(c => c.SwaggerEndpoint("/swagger/v1/swagger.json", "Library App v1"));

        // app.UseHttpsRedirection();

        app.UseAuthorization();
        app.MapControllers();

        // Liveness check
        app.UseHealthChecks("/healthz", new HealthCheckOptions { Predicate = r => r.Name == "healthy" });

        app.Run();
    }

    private static void EnsureDatabaseCreation(WebApplication app)
    {
        using var scope = app.Services.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<SqlDataContext>();
        var retryCount = 0;
        var maxRetries = 5;
        var delay = TimeSpan.FromSeconds(5);

        while (retryCount < maxRetries)
        {
            try
            {
                Console.WriteLine($"Attempting to ensure database is created (Attempt {retryCount + 1}/{maxRetries})");
                Console.WriteLine($"Attempting to run migrations");
                dbContext.Database.Migrate();
                Console.WriteLine("Database initialization/migration completed successfully");

                break;
            }
            catch (Exception ex)
            {
                retryCount++;
                Console.WriteLine($"Error initializing database: {ex.Message}. Retrying in {delay.TotalSeconds} seconds...");

                if (retryCount >= maxRetries)
                {
                    Console.WriteLine("Failed to initialize database after multiple attemps");
                    throw;
                }

                Task.Delay(delay).Wait();
                delay = TimeSpan.FromSeconds(Math.Min(delay.TotalSeconds * 2, 30));
            }
        }
    }
}