using LibraryApp.API.Books;
using LibraryApp.API.Gateway;

namespace LibraryApp.API.Authors;

public class Author : BaseEntity
{
    public string FirstName { get; set; }
    public string LastName { get; set; }
    public DateTime DateOfBirth { get; set; }
    public IList<Book> Books { get; set; }
}