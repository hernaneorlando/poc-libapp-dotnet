using Infrastructure.Common;
using Infrastructure.Persistence.Entities.RelationalDb;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.Persistence.Configurations;

public class ContributorConfiguration : RelationalDbBaseEntityConfiguration<ContributorEntity>
{
    public override void Configure(EntityTypeBuilder<ContributorEntity> builder)
    {
        builder.ToTable("contributors");
        base.Configure(builder);

        builder.Property(e => e.FirstName)
            .HasColumnName("firstname")
            .HasMaxLength(80)
            .IsRequired();

        builder.Property(e => e.LastName)
            .HasColumnName("lastname")
            .HasMaxLength(80)
            .IsRequired();
            
        builder.Property(e => e.DateOfBirth)
            .HasColumnName("date_of_birth")
            .HasColumnType("date")
            .HasConversion<DateOnlyConverter, DateOnlyComparer>()
            .IsRequired(false);

        builder.HasMany(e => e.Books)
            .WithOne(e => e.Contributor)
            .HasForeignKey(e => e.ContributorId)
            .HasConstraintName($"fk_{BookConfiguration.BookContributorsJoinTableName}_contributors_contributor_id")
            .OnDelete(DeleteBehavior.NoAction);

        builder.HasIndex(e => new { e.FirstName, e.LastName}, "idx_contributors_firstname_lastname")
            .IsUnique();
    }
}