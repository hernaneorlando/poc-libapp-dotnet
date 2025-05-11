using Application.CatalogManagement.Contributors.DTOs;
using Application.CatalogManagement.Publishers.DTOs;
using Application.Common;
using Application.LoanManagement.BookCheckouts.DTOs;
using Application.SeedWork.BaseDTO;
using Domain.CatalogManagement;
using Domain.CatalogManagement.Enums;
using Domain.CatalogManagement.ValueObjects;
using Domain.LoanManagement;

namespace Application.CatalogManagement.Books.DTOs;

public record BookDto(string Title, PublisherDto Publisher, BookStatusEnum Status) : BaseDto
{
    public string? ISBN { get; set; }
    public string? Description { get; set; }
    public int? Edition { get; set; }
    public string? Language { get; set; }
    public int? TotalPages { get; set; }
    public DateOnly? PublishedDate { get; set; }
    public CategoryDto? Category { get; set; }
    public ICollection<BookContributorDto> Contributors { get; set; } = [];
    public ICollection<BookCheckoutDto> Checkouts { get; set; } = [];

    public static implicit operator BookDto(Book book)
    {
        var bookDto = new BookDto(book.Title, (PublisherDto)book.Publisher, book.Status)
        {
            ISBN = book.ISBN?.FormattedIsbn(),
            Description = book.Description,
            Edition = book.Edition,
            Language = book.Language,
            TotalPages = book.TotalPages,
            PublishedDate = book.PublishedDate,
            Category = book.Category != null ? (CategoryDto)book.Category : null,
            Contributors = [.. book.Contributors.Select(a => (BookContributorDto)a)],
            Checkouts = [.. book.Checkouts.Select(a => (BookCheckoutDto)a)]
        };

        bookDto.ConvertModelBaseProperties(book);
        return bookDto;
    }

    public static implicit operator Book(BookDto bookDto)
    {
        var book = new Book
        {
            Title = bookDto.Title,
            Publisher = (Publisher)bookDto.Publisher,
            Status = bookDto.Status,
            ISBN = bookDto.ISBN != null ? Isbn.Create(bookDto.ISBN) : null,
            Description = bookDto.Description,
            Edition = bookDto.Edition,
            Language = bookDto.Language,
            TotalPages = bookDto.TotalPages,
            PublishedDate = bookDto.PublishedDate,
            Category = bookDto.Category != null ? (Category)bookDto.Category : null,
            Contributors = [.. bookDto.Contributors.Select(a => (BookContributor)a)],
            Checkouts = [.. bookDto.Checkouts.Select(a => (BookCheckout)a)]
        };

        book.ConvertDtoBaseProperties(bookDto);
        return book;
    }
}
