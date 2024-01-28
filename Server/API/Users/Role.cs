namespace LibraryApp.API.Users;

public sealed class Role
{
    public static readonly Role RegularUser = new Role("Regular User");
    public static readonly Role Librarian = new Role("Librarian");
    public static readonly Role Administrator = new Role("Administrator");

    private readonly string _name;

    private readonly Dictionary<string, Role> _validRoles = new Dictionary<string, Role> {
        { RegularUser._name, RegularUser },
        { Librarian._name, Librarian },
        { Administrator._name, Administrator }
    };

    private Role(string name)
    {
        _name = name;
    }

    public Role Parse(string value)
    {
        return _validRoles.ContainsKey(value)
            ? _validRoles[value]
            : throw new ArgumentException();
    }

    public override string ToString() => _name;
}