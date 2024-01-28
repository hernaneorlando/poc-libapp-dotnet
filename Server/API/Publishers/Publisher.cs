using LibraryApp.API.Gateway;

namespace LibraryApp.API.Publishers;

public class Publisher : BaseEntity
{
    public string Name { get; set; }
    public string Location { get; set; }
}