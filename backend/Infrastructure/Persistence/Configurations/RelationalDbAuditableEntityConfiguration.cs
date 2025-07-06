using Infrastructure.Persistence.SeedWork;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.Persistence.Configurations;

public abstract class RelationalDbAuditableEntityConfiguration<TEntity> : IEntityTypeConfiguration<TEntity>
    where TEntity : RelationalDbAuditableEntity
{
    public virtual void Configure(EntityTypeBuilder<TEntity> builder)
    {
        builder.Property(e => e.CreatedAt)
            .HasColumnName("created_at")
            .HasDefaultValueSql("getdate()")
            .ValueGeneratedOnAdd()
            .IsRequired();

        builder.Property(e => e.UpdatedAt)
            .HasColumnName("updated_at");
    }
}
