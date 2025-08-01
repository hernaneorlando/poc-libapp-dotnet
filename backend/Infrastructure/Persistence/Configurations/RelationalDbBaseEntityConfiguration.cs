using Domain.Common.Util;
using Infrastructure.Persistence.Common;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.Persistence.Configurations;

public abstract class RelationalDbBaseEntityConfiguration<TEntity> : RelationalDbAuditableEntityConfiguration<TEntity>
    where TEntity : RelationalDbBaseBaseEntity
{
    public override void Configure(EntityTypeBuilder<TEntity> builder)
    {
        builder.HasKey(e => e.Id)
            .HasName($"pk_{typeof(TEntity).Name.ToSnakeCaseFast()}")
            .IsClustered(false);
            
        builder.Property(e => e.Id)
            .HasColumnName("id")
            .ValueGeneratedOnAdd()
            .IsRequired();

        builder.Property(e => e.ExternalId)
            .HasDefaultValueSql("(newid())")
            .HasColumnName("external_id")
            .IsRequired();

        builder.HasIndex(e => e.ExternalId)
            .HasDatabaseName($"ux_{typeof(TEntity).Name.ToSnakeCaseFast()}_external_id")
            .IsUnique();
        
        builder.Property(e => e.Active)
            .HasColumnName("active")
            .HasDefaultValue(true)
            .IsRequired();

        base.Configure(builder);
    }
}