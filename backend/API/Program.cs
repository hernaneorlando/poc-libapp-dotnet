using Application.CatalogManagement.Authors.Services;
using Application.UserManagement.Users.Services;
using Infrastructure.Persistence.Context;
using Infrastructure.Services;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Microsoft.OpenApi.Models;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo { Title = "Library App", Version = "v1" });
});

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
builder.Services.Configure<MongoDbConfiguration>(builder.Configuration.GetSection("MongoDbDatabase"));
builder.Services.AddTransient<NoSqlDataContext>();
builder.Services.AddTransient<IAuthorService, AuthorService>();
builder.Services.AddTransient<IUserService, UserService>();

// Liveness check
builder.Services.AddHealthChecks().AddCheck("healthy", () => HealthCheckResult.Healthy());

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseDeveloperExceptionPage();
}

// Initialize and ensure database is created
using (var scope = app.Services.CreateScope())
{
    var dbContext = scope.ServiceProvider.GetRequiredService<SqlDataContext>();
    var retryCount = 0;
    var maxRetries = 5;
    var delay = TimeSpan.FromSeconds(5);

    while (retryCount < maxRetries)
    {
        try
        {
            Console.WriteLine($"Attempting to ensure database is created (Attempt {retryCount + 1}/{maxRetries})");
            dbContext.Database.EnsureCreated();
            Console.WriteLine("Database initialization completed successfully");
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

app.UseSwagger();
app.UseSwaggerUI(c => c.SwaggerEndpoint("/swagger/v1/swagger.json", "Library App v1"));

// app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

// Liveness check
app.UseHealthChecks("/healthz", new HealthCheckOptions { Predicate = r => r.Name == "healthy" });

app.Run();
