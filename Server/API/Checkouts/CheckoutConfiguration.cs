using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace LibraryApp.API.Checkouts;

public class CheckoutConfiguration : IEntityTypeConfiguration<Checkout>
{
    public void Configure(EntityTypeBuilder<Checkout> builder)
    {
        builder.ToTable("checkouts");
        builder.HasKey(e => e.Id);

        builder.Property(e => e.Id)
            .ValueGeneratedOnAdd()
            .HasColumnName("id")
            .IsRequired();

        builder.Property(e => e.UserAlias).HasColumnType("userAlias");
        builder.Property(e => e.CreatedAt).HasColumnName("createdAt");
        builder.Property(e => e.UpdatedAt).HasColumnName("updatedAt");
    }
}