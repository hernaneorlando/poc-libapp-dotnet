using Domain.SeedWork.Common.Util;
using Infrastructure.Common;
using Infrastructure.Persistence.Entities.RelationalDb;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.Persistence.Configurations;

public class BookCheckoutConfiguration : RelationalDbBaseEntityConfiguration<BookCheckoutEntity>
{
    public override void Configure(EntityTypeBuilder<BookCheckoutEntity> builder)
    {
        builder.ToTable("book_checkouts");
        base.Configure(builder);

        builder.Property(e => e.UserId)
            .HasColumnName("user_id")
            .HasMaxLength(50)
            .IsRequired();

        builder.HasIndex(e => e.UserId)
            .HasDatabaseName($"ux_{typeof(BookCheckoutEntity).Name.ToSnakeCaseFast()}_user_id")
            .IsUnique(false);

        builder.Property(e => e.CheckoutDate)
            .HasColumnName("checkout_date")
            .IsRequired();

        builder.Property(e => e.DueDate)
            .HasColumnName("due_date")
            .HasColumnType("date")
            .HasConversion<DateOnlyConverter, DateOnlyComparer>()
            .IsRequired();

        builder.Property(e => e.ReturnDate)
            .HasColumnName("return_date")
            .IsRequired();

        builder.Property(e => e.Notes)
            .HasColumnName("notes")
            .HasMaxLength(1000)
            .IsRequired(false);
        
        builder.Property(e => e.Status)
            .HasColumnName("status")
            .HasConversion<string>()
            .HasMaxLength(50)
            .IsRequired();

        builder.HasOne(e => e.Book)
            .WithMany()
            .HasForeignKey(e => e.BookId)
            .OnDelete(DeleteBehavior.NoAction)
            .IsRequired()
            .HasConstraintName("fk_book_checkout_book");

        builder.Property(e => e.BookId)
            .HasColumnName("book_id")
            .IsRequired();

        builder.HasIndex(e => e.BookId, "idx_book_checkout_book_id")
            .IsUnique(false);
    }
}