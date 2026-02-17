namespace LibraryApp.Web.Services.CatalogManagement;

public class BookDto
{
    public int Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Author { get; set; } = string.Empty;
    public string Category { get; set; } = string.Empty;
    public double Rating { get; set; }
    public DateTime DateAdded { get; set; }
    public string? ImageUrl { get; set; }
    public string ISBN { get; set; } = string.Empty;
}

public interface IBookService
{
    Task<List<BookDto>> GetLatestBooksAsync(int limit = 5);
    Task<List<BookDto>> SearchBooksAsync(string query);
}

public class BookService : IBookService
{
    private static readonly List<BookDto> MockBooks = new()
    {
        new BookDto
        {
            Id = 1,
            Title = "Clean Code",
            Author = "Robert C. Martin",
            Category = "Programação",
            Rating = 4.8,
            DateAdded = DateTime.Now.AddDays(-2),
            ISBN = "978-0132350884"
        },
        new BookDto
        {
            Id = 2,
            Title = "The Pragmatic Programmer",
            Author = "Andrew Hunt, David Thomas",
            Category = "Desenvolvimento",
            Rating = 4.7,
            DateAdded = DateTime.Now.AddDays(-5),
            ISBN = "978-0201616224"
        },
        new BookDto
        {
            Id = 3,
            Title = "Design Patterns",
            Author = "Gang of Four",
            Category = "Arquitetura",
            Rating = 4.6,
            DateAdded = DateTime.Now.AddDays(-8),
            ISBN = "978-0201633610"
        },
        new BookDto
        {
            Id = 4,
            Title = "Refactoring",
            Author = "Martin Fowler",
            Category = "Qualidade de Código",
            Rating = 4.7,
            DateAdded = DateTime.Now.AddDays(-12),
            ISBN = "978-0201485677"
        },
        new BookDto
        {
            Id = 5,
            Title = "Code Complete",
            Author = "Steve McConnell",
            Category = "Desenvolvimento",
            Rating = 4.5,
            DateAdded = DateTime.Now.AddDays(-15),
            ISBN = "978-0735619678"
        },
        new BookDto
        {
            Id = 6,
            Title = "Domain-Driven Design",
            Author = "Eric Evans",
            Category = "Arquitetura",
            Rating = 4.4,
            DateAdded = DateTime.Now.AddDays(-18),
            ISBN = "978-0321125675"
        },
        new BookDto
        {
            Id = 7,
            Title = "The Mythical Man-Month",
            Author = "Frederick P. Brooks Jr.",
            Category = "Gerenciamento",
            Rating = 4.3,
            DateAdded = DateTime.Now.AddDays(-22),
            ISBN = "978-0201835960"
        },
        new BookDto
        {
            Id = 8,
            Title = "Introduction to Algorithms",
            Author = "Cormen, Leiserson, Rivest, Stein",
            Category = "Algoritmos",
            Rating = 4.6,
            DateAdded = DateTime.Now.AddDays(-25),
            ISBN = "978-0262033848"
        }
    };

    public Task<List<BookDto>> GetLatestBooksAsync(int limit = 5)
    {
        var latest = MockBooks
            .OrderByDescending(b => b.DateAdded)
            .Take(limit)
            .ToList();

        return Task.FromResult(latest);
    }

    public Task<List<BookDto>> SearchBooksAsync(string query)
    {
        if (string.IsNullOrWhiteSpace(query))
            return Task.FromResult(new List<BookDto>());

        var results = MockBooks
            .Where(b => 
                b.Title.Contains(query, StringComparison.OrdinalIgnoreCase) ||
                b.Author.Contains(query, StringComparison.OrdinalIgnoreCase))
            .ToList();

        return Task.FromResult(results);
    }
}
