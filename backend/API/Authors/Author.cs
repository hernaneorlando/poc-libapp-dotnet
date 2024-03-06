using LibraryApp.API.Books;
using LibraryApp.API.Gateway;

namespace LibraryApp.API.Authors;

public class Author : BaseEntity
{
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; }  = string.Empty;
    public DateOnly? DateOfBirth { get; set; }
    public ICollection<Book> Books { get; set; }
}