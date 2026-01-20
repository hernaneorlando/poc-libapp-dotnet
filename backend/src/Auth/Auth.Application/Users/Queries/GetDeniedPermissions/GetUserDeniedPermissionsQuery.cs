namespace Auth.Application.Users.Queries.GetDeniedPermissions;

using MediatR;
using Auth.Application.Common;

/// <summary>
/// Query to retrieve all denied permissions for a user.
/// </summary>
public sealed record GetUserDeniedPermissionsQuery(
    /// <summary>The user ID (as string GUID)</summary>
    string UserId) : IRequest<Result<GetUserDeniedPermissionsResponse>>;

/// <summary>
/// Response containing the list of denied permissions for a user.
/// </summary>
public sealed record GetUserDeniedPermissionsResponse(
    /// <summary>The user ID</summary>
    string UserId,

    /// <summary>List of denied permissions</summary>
    IReadOnlyList<DeniedPermissionDto> DeniedPermissions);

/// <summary>
/// DTO representing a single denied permission.
/// </summary>
public sealed record DeniedPermissionDto(
    /// <summary>The permission feature</summary>
    string Feature,

    /// <summary>The permission action</summary>
    string Action);
