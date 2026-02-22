namespace LibraryApp.Web.Model.UserManagement;

public record RoleDto(
    string Name,
    string Description,
    IReadOnlyList<RolePermissionDto> Permissions) : BaseDto;

public sealed record RolePermissionDto(
    string Feature,
    string Action);