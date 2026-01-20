namespace Auth.Infrastructure.Data;

/// <summary>
/// Design-time factory for AuthDbContext used by Entity Framework Core tools.
/// This allows migrations to be created without requiring the full application startup.
/// </summary>
public sealed class AuthDbContextFactory : IDesignTimeDbContextFactory<AuthDbContext>
{
    public AuthDbContext CreateDbContext(string[] args)
    {
        var optionsBuilder = new DbContextOptionsBuilder<AuthDbContext>();

        optionsBuilder.UseSqlServer(
            "Server=.;Database=LibraryAppDb;Trusted_Connection=true;Encrypt=false",
            sqlOptions =>
            {
                sqlOptions.MigrationsHistoryTable("__EFMigrationsHistory", AuthDbContext.DefaultSchema);
                sqlOptions.CommandTimeout(30);
            });

        return new AuthDbContext(optionsBuilder.Options);
    }
}
