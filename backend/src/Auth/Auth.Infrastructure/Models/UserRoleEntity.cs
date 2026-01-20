namespace Auth.Infrastructure.Models;

/// <summary>
/// Junction table entity for User-Role many-to-many relationship.
/// </summary>
public sealed class UserRoleEntity
{
    public Guid Id { get; set; }
    public Guid UserId { get; set; }
    public Guid RoleId { get; set; }
    public DateTime AssignedAt { get; set; }

    /// <summary>
    /// Navigation properties for the relationship.
    /// </summary>
    public UserEntity? User { get; set; }
    public RoleEntity? Role { get; set; }
}
