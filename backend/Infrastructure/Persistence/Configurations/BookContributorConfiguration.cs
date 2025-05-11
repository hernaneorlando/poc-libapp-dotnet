using Domain.CatalogManagement.Enums;
using Infrastructure.Persistence.Entities.RelationalDb;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.Persistence.Configurations;

public class BookContributorConfiguration : RelationalDbAuditableEntityConfiguration<BookContributorEntity>
{
    public override void Configure(EntityTypeBuilder<BookContributorEntity> builder)
    {
        builder.ToTable("book_contributors");

        base.Configure(builder);

        builder.HasKey(e => new { e.BookId, e.ContributorId })
            .HasName("pk_book_contributors_entity");

        builder.Property(e => e.ContributorId)
            .HasColumnName("contributor_id")
            .IsRequired();

        builder.HasIndex(e => e.ContributorId, "ix_book_contributor_contributor_id")
            .IsUnique(false);

        builder.Property(e => e.BookId)
            .HasColumnName("book_id")
            .IsRequired();

        builder.HasIndex(e => e.BookId, "ix_book_contributor_book_id")
            .IsUnique(false);

        builder.Property(e => e.Role)
            .HasColumnName("role")
            .HasConversion(
                v => v.ToString(),
                v => (ContributorRoleEnum)Enum.Parse(typeof(ContributorRoleEnum), v))
            .IsRequired();

        builder.HasIndex(e => new { e.BookId, e.ContributorId }, "ix_book_contributor_book_id_contributor_id")
            .IsUnique()
            .HasFilter(null);
    }
}