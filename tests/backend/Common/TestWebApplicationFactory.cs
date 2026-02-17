using Auth.Application.Common.Security;
using Auth.Application.Common.Security.Interfaces;
using Auth.Domain;
using Auth.Domain.Services;
using Auth.Infrastructure.Data;
using Auth.Infrastructure.Repositories;
using Auth.Infrastructure.Repositories.Interfaces;
using LibraryApp.API;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace Common;

public class TestWebApplicationFactory : WebApplicationFactory<Program>
{
    // Store the database name to ensure all scopes use the same in-memory database
    private readonly string _databaseName = $"LibraryApp_Test_{Guid.NewGuid()}";

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.UseEnvironment("IntegrationTests");
        builder.ConfigureServices(services =>
        {
            // Remove ALL DbContext registrations from infrastructure layer
            var descriptorsToRemove = services
                .Where(d => d.ServiceType == typeof(DbContextOptions<AuthDbContext>) ||
                            d.ServiceType == typeof(DbContextOptions) ||
                            d.ServiceType == typeof(AuthDbContext) ||
                            d.ServiceType.Name.Contains("DbContextOptions"))
                .ToList();

            foreach (var descriptor in descriptorsToRemove)
            {
                services.Remove(descriptor);
            }

            // Add In-Memory DbContext with SAME database name for all scopes in this test
            services.AddDbContext<AuthDbContext>(options =>
            {
                options.UseInMemoryDatabase(databaseName: _databaseName);
            }, ServiceLifetime.Singleton);
            
            // Register MediatR for request handling
            services.AddMediatR(cfg => cfg.RegisterServicesFromAssembly(typeof(Auth.Application.DependencyInjections).Assembly));
            
            // Register repositories and security services for testing
            services.AddScoped<IUserRepository, UserRepository>();
            services.AddScoped<IRoleRepository, RoleRepository>();
            services.AddScoped<IUnitOfWork, UnitOfWork>();
            services.AddScoped<IPasswordHasher, PasswordHasher>();
            services.AddScoped<ITokenService, TokenService>();
            services.AddScoped<IAuthorizationService, AuthorizationService>();
            
            // Register JWT settings for token generation
            var configuration = services.BuildServiceProvider().GetRequiredService<IConfiguration>();
            var jwtSettings = new JwtSettings
            {
                Issuer = configuration["Jwt:Issuer"] ?? "LibraryApp",
                Audience = configuration["Jwt:Audience"] ?? "LibraryAppUsers",
                SecretKey = configuration["Jwt:SecretKey"] ?? "your-test-secret-key-min-32-chars",
                TokenExpiryInMinutes = int.Parse(configuration["Jwt:TokenExpiryInMinutes"] ?? "15"),
                RefreshTokenExpiryInDays = int.Parse(configuration["Jwt:RefreshTokenExpiryInDays"] ?? "7"),
                RefreshTokenSlidingExpiryInMinutes = int.Parse(configuration["Jwt:RefreshTokenSlidingExpiryInMinutes"] ?? "20")
            };
            services.AddSingleton(jwtSettings);
        });
    }
}
