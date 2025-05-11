using Domain.SeedWork;

namespace Domain.UserManagement;

public class Permission(string code, string description) : DocumentDbModel
{
    public string Code { get; set; } = code;
    public string Description { get; set; } = description;
}