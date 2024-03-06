namespace LibraryAppWeb.Web.Pages.Users;

public class User
{
    public string Alias { get; set; }
    public string FirstName { get; set; }
    public string LastName { get; set; }
    public string Email { get; set; }
    public Role Role { get; set; }
}

public class Role
{
    public string Name { get; set; }
}