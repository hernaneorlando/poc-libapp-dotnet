using Domain.Common;

namespace Domain.UserManagement;

public class Permission : DocumentDbModel
{
    public required string Code { get; set; }
    public string Description { get; set; } = string.Empty;
}