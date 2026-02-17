using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.Extensions.Configuration;

namespace Auth.Infrastructure.Data;

/// <summary>
/// Design-time factory for AuthDbContext used by Entity Framework Core tools.
/// This allows migrations to be created without requiring the full application startup.
/// Reads connection string from appsettings configuration files.
/// </summary>
public sealed class AuthDbContextFactory : IDesignTimeDbContextFactory<AuthDbContext>
{
    public AuthDbContext CreateDbContext(string[] args)
    {
        // Build configuration to read appsettings from the API project
        var basePath = Path.Combine(Directory.GetCurrentDirectory(), "../API");
        var configuration = new ConfigurationBuilder()
            .SetBasePath(basePath)
            .AddJsonFile("appsettings.json")
            .AddJsonFile($"appsettings.{Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") ?? "Development"}.json", optional: true)
            .Build();

        // Get connection string from configuration
        var connectionString = configuration.GetConnectionString("SqlConnectionString")
            ?? throw new InvalidOperationException("Connection string 'SqlConnectionString' not found in configuration. Check appsettings.json.");

        // Create and configure DbContext options
        var optionsBuilder = new DbContextOptionsBuilder<AuthDbContext>();
        optionsBuilder.UseSqlServer(connectionString, sqlOptions =>
        {
            sqlOptions.MigrationsHistoryTable("__EFMigrationsHistory", AuthDbContext.DefaultSchema);
            sqlOptions.CommandTimeout(30);
            sqlOptions.EnableRetryOnFailure(
                maxRetryCount: 3,
                maxRetryDelay: TimeSpan.FromSeconds(15),
                errorNumbersToAdd: null);
        });

        return new AuthDbContext(optionsBuilder.Options);
    }
}
