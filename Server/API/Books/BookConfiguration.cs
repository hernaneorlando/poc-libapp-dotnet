using LibraryApp.API.Authors;
using LibraryApp.API.Checkouts;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace LibraryApp.API.Books;

public class BookConfiguration : IEntityTypeConfiguration<Book>
{
    public void Configure(EntityTypeBuilder<Book> builder)
    {
        builder.ToTable("books");
        builder.HasKey(e => e.Id);

        builder.Property(e => e.Id)
            .ValueGeneratedOnAdd()
            .HasColumnName("id")
            .IsRequired();

        builder.Property(e => e.Title).HasColumnName("title");
        builder.Property(e => e.Description).HasColumnName("lastName");
        builder.Property(e => e.TotalPages).HasColumnName("dateOfBirth");
        builder.Property(e => e.ISBN).HasColumnName("dateOfBirth");
        builder.Property(e => e.PublishedDate).HasColumnName("publishedDate");
        builder.Property(e => e.CreatedAt).HasColumnName("createdAt");
        builder.Property(e => e.UpdatedAt).HasColumnName("updatedAt");

        builder.HasOne(e => e.Category);
        builder.HasOne(e => e.Publisher);

        builder.HasMany(e => e.Authors)
            .WithMany(e => e.Books)
            .UsingEntity(
                "BookAuthor",
                l => l.HasOne(typeof(Book)).WithMany().HasForeignKey("bookId").HasPrincipalKey(nameof(Book.Id)),
                r => r.HasOne(typeof(Author)).WithMany().HasForeignKey("authorId").HasPrincipalKey(nameof(Author.Id)),
                j => j.HasKey("bookId", "authorId")
            );

        builder.HasMany(e => e.Checkouts)
            .WithMany(e => e.Books)
            .UsingEntity(
                "BookCheckout",
                l => l.HasOne(typeof(Book)).WithMany().HasForeignKey("bookId").HasPrincipalKey(nameof(Book.Id)),
                r => r.HasOne(typeof(Checkout)).WithMany().HasForeignKey("checkoutId").HasPrincipalKey(nameof(Checkout.Id)),
                j => j.HasKey("bookId", "checkoutId")
            );
    }
}