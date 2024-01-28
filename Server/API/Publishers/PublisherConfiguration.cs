using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace LibraryApp.API.Publishers;

public class PublisherConfiguration : IEntityTypeConfiguration<Publisher>
{
    public void Configure(EntityTypeBuilder<Publisher> builder)
    {
        builder.ToTable("checkouts");
        builder.HasKey(e => e.Id);

        builder.Property(e => e.Id)
            .ValueGeneratedOnAdd()
            .HasColumnName("id")
            .IsRequired();

        builder.Property(e => e.Name).HasColumnName("name");
        builder.Property(e => e.Location).HasColumnName("location");
        builder.Property(e => e.CreatedAt).HasColumnName("createdAt");
        builder.Property(e => e.UpdatedAt).HasColumnName("updatedAt");
    }
}