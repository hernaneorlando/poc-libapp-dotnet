using Infrastructure.Common;
using Infrastructure.Persistence.Entities.RelationalDb;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.Persistence.Configurations;

public class BookConfiguration : RelationalDbBaseEntityConfiguration<BookEntity>
{
    public static readonly string BookContributorsJoinTableName = "book_contributors";

    public override void Configure(EntityTypeBuilder<BookEntity> builder)
    {
        builder.ToTable("books");
        base.Configure(builder);

        builder.Property(e => e.Title)
            .HasColumnName("title")
            .HasMaxLength(200)
            .IsRequired();

        builder.Property(e => e.ISBN)
            .HasColumnName("isbn")
            .HasMaxLength(13)
            .IsRequired(false);

        builder.HasIndex(e => e.ISBN, "ix_book_isbn")
            .IsUnique();

        builder.Property(e => e.Description)
            .HasColumnName("description")
            .HasMaxLength(3000)
            .IsRequired(false);

        builder.Property(e => e.Edition)
            .HasColumnName("edition")
            .IsRequired(false);

        builder.Property(e => e.Language)
            .HasColumnName("language")
            .HasMaxLength(50)
            .IsRequired(false);

        builder.Property(e => e.TotalPages)
            .HasColumnName("total_pages")
            .IsRequired(false);

        builder.Property(e => e.PublishedDate)
            .HasColumnName("published_date")
            .HasColumnType("date")
            .HasConversion<DateOnlyConverter, DateOnlyComparer>()
            .IsRequired(false);

        builder.Property(e => e.Status)
            .HasColumnName("status")
            .HasConversion<string>()
            .HasMaxLength(50)
            .IsRequired();

        builder.HasOne(e => e.Category)
            .WithMany(e => e.Books)
            .HasForeignKey(e => e.CategoryId)
            .HasConstraintName("fk_books_categories_category_id")
            .OnDelete(DeleteBehavior.NoAction)
            .IsRequired(false);

        builder.Property(e => e.CategoryId)
            .HasColumnName("category_id")
            .IsRequired(false);

        builder.HasIndex(e => e.CategoryId, "ix_book_category_id")
            .IsUnique(false);

        builder.HasOne(e => e.Publisher)
            .WithMany(e => e.Books)
            .HasForeignKey(e => e.PublisherId)
            .OnDelete(DeleteBehavior.NoAction)
            .HasConstraintName("fk_book_publisher")
            .IsRequired(false);

        builder.Property(e => e.PublisherId)
            .HasColumnName("publisher_id")
            .IsRequired(false);

        builder.HasIndex(e => e.PublisherId, "ix_book_publisher_id")
            .IsUnique(false);

        builder.HasMany(e => e.Contributors)
            .WithOne(e => e.Book)
            .HasForeignKey(e => e.BookId)
            .HasConstraintName("fk_book_contributors_books_book_id")
            .OnDelete(DeleteBehavior.NoAction);

        builder.HasMany(e => e.Checkouts)
            .WithOne(e => e.Book)
            .HasForeignKey(e => e.BookId)
            .OnDelete(DeleteBehavior.NoAction)
            .HasConstraintName("fk_book_checkout_book")
            .IsRequired(false);
    }
}