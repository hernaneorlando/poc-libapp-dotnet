using LibraryApp.API.Authors;
using LibraryApp.API.Checkouts;
using LibraryApp.API.Gateway;
using LibraryApp.API.Publishers;

namespace LibraryApp.API.Books;

public class Book : BaseEntity
{
    public string Title { get; set; }
    public string Description { get; set; }
    public int TotalPages { get; set; }
    public string ISBN { get; set; }
    public Category Category { get; set; }
    public DateTime PublishedDate { get; set; }
    public Publisher Publisher { get; set; }
    public IList<Author> Authors { get; set; }
    public IList<Checkout> Checkouts { get; set; }
}