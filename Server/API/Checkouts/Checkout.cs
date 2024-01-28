using LibraryApp.API.Books;
using LibraryApp.API.Gateway;

namespace LibraryApp.API.Checkouts;

public class Checkout : BaseEntity
{
    public string UserAlias { get; set; }
    public IList<Book> Books { get; set; }
}