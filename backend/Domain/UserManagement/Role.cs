using Domain.Common;

namespace Domain.UserManagement;

public class Role(string name, string description) : DocumentDbModel<Role>
{
    public string Name { get; set; } = name;
    public string Description { get; set; } = description;
    public ICollection<Permission> Permissions { get; set; } = [];
}