namespace Auth.Infrastructure.Models;

using System.Text.Json;
using Auth.Domain.Aggregates.Permission;
using Auth.Domain.Aggregates.Role;
using Auth.Domain.Enums;

/// <summary>
/// Relational entity for Role aggregate persistence.
/// Uses Data Mapper pattern with implicit operators for seamless conversion.
/// Permissions stored as JSON in database.
/// </summary>
public sealed class RoleEntity
{
    public Guid Id { get; set; }
    public required string Name { get; set; }
    public string? Description { get; set; }
    public int Version { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
    public bool IsActive { get; set; }

    /// <summary>
    /// Permissions stored as JSON serialized data.
    /// </summary>
    public string PermissionsJson { get; set; } = "[]";

    /// <summary>
    /// Navigation properties for junction tables.
    /// </summary>
    public ICollection<UserRoleEntity> UserRoles { get; set; } = [];

    /// <summary>
    /// Converts relational entity to domain aggregate root (Entity → Domain).
    /// Direct property mapping without reflection - simple and performant.
    /// </summary>
    public static implicit operator Role(RoleEntity entity)
    {
        var role = new Role
        {
            Id = RoleId.From(entity.Id),
            Name = entity.Name,
            Description = entity.Description ?? string.Empty,
            Version = entity.Version,
            CreatedAt = entity.CreatedAt,
            UpdatedAt = entity.UpdatedAt,
            IsActive = entity.IsActive
        };

        role.AssignPermission(DeserializePermissions(entity.PermissionsJson));

        return role;
    }

    /// <summary>
    /// Converts domain aggregate root to relational entity (Domain → Entity).
    /// Direct property mapping - simple and performant.
    /// </summary>
    public static implicit operator RoleEntity(Role role)
    {
        return new RoleEntity
        {
            Id = role.Id.Value,
            Name = role.Name,
            Description = role.Description,
            Version = role.Version,
            CreatedAt = role.CreatedAt,
            UpdatedAt = role.UpdatedAt ?? DateTime.UtcNow,
            IsActive = role.IsActive,
            PermissionsJson = SerializePermissions(role.Permissions)
        };
    }

    /// <summary>
    /// Deserializes permissions from JSON string.
    /// </summary>
    private static List<Permission> DeserializePermissions(string json)
    {
        if (string.IsNullOrEmpty(json) || json == "[]")
            return [];

        try
        {
            using var jsonDoc = JsonDocument.Parse(json);
            var permissions = new List<Permission>();

            foreach (var element in jsonDoc.RootElement.EnumerateArray())
            {
                if (element.TryGetProperty("Feature", out var featureElement) &&
                    element.TryGetProperty("Action", out var actionElement))
                {
                    var featureStr = featureElement.GetString();
                    var actionStr = actionElement.GetString();

                    if (!string.IsNullOrEmpty(featureStr) && !string.IsNullOrEmpty(actionStr) &&
                        Enum.TryParse<PermissionFeature>(featureStr, ignoreCase: true, out var feature) &&
                        Enum.TryParse<PermissionAction>(actionStr, ignoreCase: true, out var action))
                    {
                        permissions.Add(new Permission(feature, action));
                    }
                }
            }

            return permissions;
        }
        catch
        {
            return [];
        }
    }

    /// <summary>
    /// Serializes permissions to JSON string.
    /// </summary>
    private static string SerializePermissions(List<Permission> permissions)
    {
        if (permissions.Count == 0)
            return "[]";

        try
        {
            var permissionDtos = permissions.Select(p => new
            {
                Feature = p.Feature.ToString(),
                Action = p.Action.ToString()
            }).ToList();

            return JsonSerializer.Serialize(permissionDtos);
        }
        catch
        {
            return "[]";
        }
    }
}
