using LibraryApp.API.Authors;
using LibraryApp.API.Books;
using LibraryApp.API.Checkouts;
using LibraryApp.API.Publishers;
using Microsoft.EntityFrameworkCore;

namespace LibraryApp.API.Gateway;

public class SqlDataContext : DbContext
{
    public DbSet<Author> Authors { get; set; }
    public DbSet<Book> Books { get; set; }
    public DbSet<Category> Categories { get; set; }
    public DbSet<Checkout> Checkouts {get;set;}
    public DbSet<Publisher> Publishers { get; set; }

    public SqlDataContext(DbContextOptions<SqlDataContext> options) : base(options)
    {
        ChangeTracker.Tracked += (s, e) =>
        {
            if (e.FromQuery)
            {
                FixDateTime(e);
            }
        };
    }

    private void FixDateTime(object entity)
    {
        var dateTimeProperties = entity
            .GetType()
            .GetProperties()
            .Where(p => p.PropertyType == typeof(DateTime) || p.PropertyType == typeof(DateTime?));

        if (dateTimeProperties.Any())
        {
            foreach (var property in dateTimeProperties)
            {
                var dateTimeValue = property.GetValue(entity) as DateTime?;
                if (dateTimeValue.HasValue && dateTimeValue.Value != default && dateTimeValue.Value.Kind == DateTimeKind.Unspecified)
                {
                    property.SetValue(entity, DateTime.SpecifyKind(dateTimeValue.Value, DateTimeKind.Utc));
                }
            }
        }
    }
}
