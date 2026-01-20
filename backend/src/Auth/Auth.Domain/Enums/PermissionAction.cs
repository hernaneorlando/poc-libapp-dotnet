namespace Auth.Domain.Enums;

/// <summary>
/// Represents the actions that can be performed on protected resources.
/// </summary>
public enum PermissionAction
{
    /// <summary>Create new resource</summary>
    Create = 1,

    /// <summary>Read/view resource</summary>
    Read = 2,

    /// <summary>Update existing resource</summary>
    Update = 3,

    /// <summary>Delete resource</summary>
    Delete = 4
}
