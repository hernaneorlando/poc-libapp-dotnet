using LibraryApp.API.Books;
using LibraryApp.API.Gateway;

namespace LibraryApp.API.Checkouts;

public class Checkout : BaseEntity
{
    public string UserId { get; set; }
    public Book Book { get; set; }
}