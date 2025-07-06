using Infrastructure.Common;
using Infrastructure.Persistence.Entities.RelationalDb;
using Microsoft.EntityFrameworkCore;
using System.Reflection;

namespace Infrastructure.Persistence.Context;

public class SqlDataContext(DbContextOptions<SqlDataContext> options) : DbContext(options)
{
    public DbSet<ContributorEntity> Contributors { get; set; }
    public DbSet<BookContributorEntity> BookContributors { get; set; }
    public DbSet<BookEntity> Books { get; set; }
    public DbSet<CategoryEntity> Categories { get; set; }
    public DbSet<PublisherEntity> Publishers { get; set; }
    public DbSet<BookCheckoutEntity> BookCheckouts { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfigurationsFromAssembly(Assembly.GetExecutingAssembly());
    }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        optionsBuilder.AddInterceptors(new AuditInterceptor());
    }

    protected override void ConfigureConventions(ModelConfigurationBuilder configurationBuilder)
    {
        base.ConfigureConventions(configurationBuilder);
        configurationBuilder.Properties<DateOnly>()
            .HaveConversion<DateOnlyConverter>()
            .HaveColumnType("date");
    }
}
