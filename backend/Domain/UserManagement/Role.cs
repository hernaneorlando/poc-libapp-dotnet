using Domain.SeedWork;

namespace Domain.UserManagement;

public class Role(string name, string description) : DocumentDbModel
{
    public string Name { get; set; } = name;
    public string Description { get; set; } = description;
    public List<Permission> Permissions { get; set; } = [];
}