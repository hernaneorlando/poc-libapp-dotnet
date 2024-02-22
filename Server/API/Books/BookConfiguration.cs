using LibraryApp.API.Authors;
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
            .HasDefaultValueSql("(newid())")
            .HasColumnName("id");

        builder.Property(e => e.Title)
            .HasColumnName("title")
            .IsRequired();

        builder.Property(e => e.ISBN)
            .HasColumnName("isbn")
            .IsRequired();
        
        builder.Property(e => e.Description).HasColumnName("description");
        builder.Property(e => e.TotalPages).HasColumnName("totalPages");
        builder.Property(e => e.PublishedDate).HasColumnName("publishedDate").HasColumnType("date");
        builder.Property(e => e.CreatedAt).HasColumnName("createdAt");
        builder.Property(e => e.UpdatedAt).HasColumnName("updatedAt");

        builder.HasOne(e => e.Category)
            .WithMany()
            .HasForeignKey("categoryId")
            .OnDelete(DeleteBehavior.SetNull);

        builder.HasOne(e => e.Publisher)
            .WithMany()
            .HasForeignKey("publisherId")
            .OnDelete(DeleteBehavior.Restrict)
            .IsRequired();

        builder.HasOne(e => e.MainAuthor)
            .WithMany()
            .HasForeignKey("mainAuthorId")
            .OnDelete(DeleteBehavior.SetNull);

        builder.HasMany(e => e.Authors)
            .WithMany(e => e.Books)
            .UsingEntity(
                "BooksAuthor",
                book => book.HasOne(typeof(Author)).WithMany().HasForeignKey("authorId").HasPrincipalKey(nameof(Author.Id)),
                author => author.HasOne(typeof(Book)).WithMany().HasForeignKey("bookId").HasPrincipalKey(nameof(Book.Id)),
                joinTable => joinTable.ToTable("booksAuthors").HasKey("bookId", "authorId")
            );
    }
}