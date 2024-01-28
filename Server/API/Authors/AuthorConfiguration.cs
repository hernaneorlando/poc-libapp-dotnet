using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace LibraryApp.API.Authors;

public class AuthorConfiguration : IEntityTypeConfiguration<Author>
{
    public void Configure(EntityTypeBuilder<Author> builder)
    {
        builder.ToTable("authors");
        builder.HasKey(e => e.Id);

        builder.Property(e => e.Id)
            .ValueGeneratedOnAdd()
            .HasColumnName("id")
            .IsRequired();

        builder.Property(e => e.FirstName).HasColumnName("firstName");
        builder.Property(e => e.LastName).HasColumnName("lastName");
        builder.Property(e => e.DateOfBirth).HasColumnName("dateOfBirth");
        builder.Property(e => e.CreatedAt).HasColumnName("createdAt");
        builder.Property(e => e.UpdatedAt).HasColumnName("updatedAt");
    }
}