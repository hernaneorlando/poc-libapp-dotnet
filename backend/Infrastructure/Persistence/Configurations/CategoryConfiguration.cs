using Domain.SeedWork.Common.Util;
using Infrastructure.Persistence.Entities.RelationalDb;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.Persistence.Configurations;

public class CategoryConfiguration : RelationalDbBaseEntityConfiguration<CategoryEntity>
{
    public override void Configure(EntityTypeBuilder<CategoryEntity> builder)
    {
        builder.ToTable("categories");
        base.Configure(builder);

        builder.Property(e => e.Name)
            .HasColumnName("name")
            .HasMaxLength(100)
            .IsRequired();
        
        builder.HasIndex(e => e.Name)
            .IsUnique()
            .HasDatabaseName($"ux_{typeof(CategoryEntity).Name.ToSnakeCaseFast()}_name");

        builder.Property(e => e.Description)
            .HasColumnName("description")
            .HasMaxLength(300)
            .IsRequired(false);
        
        builder.HasMany(e => e.Books)
            .WithOne(e => e.Category)
            .HasForeignKey(e => e.CategoryId)
            .OnDelete(DeleteBehavior.NoAction);
    }
}