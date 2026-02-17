using Core.Infrastructure;
using Microsoft.EntityFrameworkCore.Diagnostics;

namespace Auth.Infrastructure.Data;

/// <summary>
/// DbContext for the Auth bounded context.
/// Manages relational entities for User, Role, and Permission aggregates.
/// Uses the "auth" schema in SQL Server.
/// Decoupled from domain entities to ensure persistence ignorance.
/// </summary>
public sealed class AuthDbContext(DbContextOptions<AuthDbContext> options) : DbContext(options)
{
    public const string DefaultSchema = "auth";

    public DbSet<UserEntity> Users => Set<UserEntity>();
    public DbSet<RoleEntity> Roles => Set<RoleEntity>();
    public DbSet<UserRoleEntity> UserRoles => Set<UserRoleEntity>();
    public DbSet<RefreshTokenEntity> RefreshTokens => Set<RefreshTokenEntity>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // Set default schema for all entities
        modelBuilder.HasDefaultSchema(DefaultSchema);

        // Apply entity configurations
        modelBuilder.ApplyConfiguration(new UserEntityConfiguration());
        modelBuilder.ApplyConfiguration(new RoleEntityConfiguration());
        modelBuilder.ApplyConfiguration(new UserRoleEntityConfiguration());
        modelBuilder.ApplyConfiguration(new RefreshTokenEntityConfiguration());
    }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        optionsBuilder.AddInterceptors(new AuditInterceptor());
        optionsBuilder.ConfigureWarnings(warnings =>
            warnings.Ignore(RelationalEventId.PendingModelChangesWarning));
    }

    protected override void ConfigureConventions(ModelConfigurationBuilder configurationBuilder)
    {
        base.ConfigureConventions(configurationBuilder);
        configurationBuilder.Properties<DateOnly>()
            .HaveConversion<DateOnlyConverter>()
            .HaveColumnType("date");
    }
}
