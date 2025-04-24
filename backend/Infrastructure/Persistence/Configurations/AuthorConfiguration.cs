using Domain.CatalogManagement;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.Persistence.Configurations;

public class AuthorConfiguration : IEntityTypeConfiguration<Author>
{
    public void Configure(EntityTypeBuilder<Author> builder)
    {
        builder.ToTable("authors");
        builder.HasKey(e => e.Id);

        builder.Property(e => e.Id)
            .HasDefaultValueSql("(newid())")
            .HasColumnName("id");

        builder.Property(e => e.FirstName)
            .HasColumnName("firstName")
            .IsRequired();

        builder.Property(e => e.LastName)
            .HasColumnName("lastName")
            .IsRequired();
            
        builder.Property(e => e.DateOfBirth).HasColumnName("dateOfBirth").HasColumnType("date");;
        builder.Property(e => e.CreatedAt).HasColumnName("createdAt");
        builder.Property(e => e.UpdatedAt).HasColumnName("updatedAt");
    }
}