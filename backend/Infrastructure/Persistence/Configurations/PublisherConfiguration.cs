using Infrastructure.Common;
using Infrastructure.Persistence.Entities.RelationalDb;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.Persistence.Configurations;

public class PublisherConfiguration : RelationalDbBaseEntityConfiguration<PublisherEntity>
{
    public override void Configure(EntityTypeBuilder<PublisherEntity> builder)
    {
        builder.ToTable("publishers");
        base.Configure(builder);

        builder.Property(e => e.Name)
            .HasColumnName("name")
            .HasMaxLength(100)
            .IsRequired();
        
        builder.Property(e => e.FoundationDate)
            .HasColumnName("foundation_date")
            .HasColumnType("date")
            .HasConversion<DateOnlyConverter, DateOnlyComparer>()
            .IsRequired(false);

        builder.Property(e => e.PhoneNumber)
            .HasColumnName("phone_number")
            .HasMaxLength(20)
            .IsRequired(false);

        builder.Property(e => e.Email)
            .HasColumnName("email")
            .HasMaxLength(100)
            .IsRequired(false);

        builder.Property(e => e.Website)
            .HasColumnName("website")
            .HasMaxLength(200)
            .IsRequired(false);
        
        builder.HasMany(e => e.Books)
            .WithOne(e => e.Publisher)
            .HasForeignKey(e => e.PublisherId)
            .OnDelete(DeleteBehavior.NoAction)
            .IsRequired(false);
    }
}