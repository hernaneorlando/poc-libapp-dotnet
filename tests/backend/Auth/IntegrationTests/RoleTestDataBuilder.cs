using Auth.Application.Roles.Commands.CreateRole;
using Bogus;

namespace IntegrationTests.Auth.IntegrationTests;

/// <summary>
/// Builder/Faker for test data related to role creation.
/// Uses Bogus library to generate realistic test data.
/// Shared across all Auth integration and unit tests.
/// </summary>
public static class RoleTestDataBuilder
{
    /// <summary>
    /// Creates a Faker for RolePermissionRequest.
    /// </summary>
    public static Faker<RolePermissionRequest> CreateRolePermissionRequestFaker() =>
        new Faker<RolePermissionRequest>()
            .CustomInstantiator(f => new RolePermissionRequest(
                Feature: f.PickRandom(new[] { "User", "Book", "Role", "Category" }),
                Action: f.PickRandom(new[] { "Create", "Read", "Update", "Delete" })
            ));

    /// <summary>
    /// Creates a Faker for CreateRoleCommand with valid defaults.
    /// </summary>
    public static Faker<CreateRoleCommand> CreateCreateRoleCommandFaker() =>
        new Faker<CreateRoleCommand>()
            .CustomInstantiator(f => new CreateRoleCommand(
                Name: f.Name.FirstName() + " Role",
                Description: f.Lorem.Sentences(2).Replace("\n", " "),
                Permissions: CreateRolePermissionRequestFaker()
                    .Generate(f.Random.Int(1, 3))
                    .DistinctBy(p => $"{p.Feature}:{p.Action}")
                    .ToList()
            ));

    /// <summary>
    /// Creates a single valid RolePermissionRequest.
    /// </summary>
    public static RolePermissionRequest GenerateRolePermissionRequest(
        string? feature = null,
        string? action = null)
    {
        var faker = CreateRolePermissionRequestFaker();
        return new RolePermissionRequest(
            Feature: feature ?? faker.Generate().Feature,
            Action: action ?? faker.Generate().Action
        );
    }

    /// <summary>
    /// Creates multiple valid RolePermissionRequest objects.
    /// </summary>
    public static List<RolePermissionRequest> GenerateRolePermissionRequests(int count = 3)
    {
        var faker = CreateRolePermissionRequestFaker();
        return faker.Generate(count)
            .DistinctBy(p => $"{p.Feature}:{p.Action}")
            .ToList();
    }

    /// <summary>
    /// Creates a single valid CreateRoleCommand.
    /// </summary>
    public static CreateRoleCommand GenerateCreateRoleCommand(
        string? name = null,
        string? description = null,
        List<RolePermissionRequest>? permissions = null)
    {
        var faker = CreateCreateRoleCommandFaker();
        var generated = faker.Generate();

        return new CreateRoleCommand(
            Name: name ?? generated.Name,
            Description: description ?? generated.Description,
            Permissions: permissions ?? generated.Permissions
        );
    }

    /// <summary>
    /// Creates a valid CreateRoleCommand with no permissions.
    /// </summary>
    public static CreateRoleCommand GenerateCreateRoleCommandWithoutPermissions()
    {
        return GenerateCreateRoleCommand(
            permissions: new List<RolePermissionRequest>()
        );
    }

    /// <summary>
    /// Creates a valid CreateRoleCommand with specific permissions.
    /// </summary>
    public static CreateRoleCommand GenerateCreateRoleCommandWithPermissions(
        params (string Feature, string Action)[] permissions)
    {
        var permissionRequests = permissions
            .Select(p => new RolePermissionRequest(p.Feature, p.Action))
            .ToList();

        return GenerateCreateRoleCommand(permissions: permissionRequests);
    }

    /// <summary>
    /// Creates an invalid CreateRoleCommand with empty name.
    /// </summary>
    public static CreateRoleCommand GenerateCreateRoleCommandWithEmptyName()
    {
        return GenerateCreateRoleCommand(
            name: "",
            permissions: GenerateRolePermissionRequests(1)
        );
    }

    /// <summary>
    /// Creates an invalid CreateRoleCommand with short name.
    /// </summary>
    public static CreateRoleCommand GenerateCreateRoleCommandWithShortName()
    {
        return GenerateCreateRoleCommand(
            name: "Ad",
            permissions: GenerateRolePermissionRequests(1)
        );
    }

    /// <summary>
    /// Creates an invalid CreateRoleCommand with long name.
    /// </summary>
    public static CreateRoleCommand GenerateCreateRoleCommandWithLongName()
    {
        return GenerateCreateRoleCommand(
            name: new string('A', 51),
            permissions: GenerateRolePermissionRequests(1)
        );
    }

    /// <summary>
    /// Creates an invalid CreateRoleCommand with empty description.
    /// </summary>
    public static CreateRoleCommand GenerateCreateRoleCommandWithEmptyDescription()
    {
        return GenerateCreateRoleCommand(
            description: "",
            permissions: GenerateRolePermissionRequests(1)
        );
    }

    /// <summary>
    /// Creates an invalid CreateRoleCommand with short description.
    /// </summary>
    public static CreateRoleCommand GenerateCreateRoleCommandWithShortDescription()
    {
        return GenerateCreateRoleCommand(
            description: "Short",
            permissions: GenerateRolePermissionRequests(1)
        );
    }

    /// <summary>
    /// Creates an invalid CreateRoleCommand with long description.
    /// </summary>
    public static CreateRoleCommand GenerateCreateRoleCommandWithLongDescription()
    {
        return GenerateCreateRoleCommand(
            description: new string('D', 501),
            permissions: GenerateRolePermissionRequests(1)
        );
    }

    /// <summary>
    /// Creates an invalid CreateRoleCommand with invalid permission feature.
    /// </summary>
    public static CreateRoleCommand GenerateCreateRoleCommandWithInvalidFeature(string feature = "InvalidFeature")
    {
        return GenerateCreateRoleCommand(
            permissions: new List<RolePermissionRequest> { new(feature, "Read") }
        );
    }

    /// <summary>
    /// Creates an invalid CreateRoleCommand with invalid permission action.
    /// </summary>
    public static CreateRoleCommand GenerateCreateRoleCommandWithInvalidAction(string action = "InvalidAction")
    {
        return GenerateCreateRoleCommand(
            permissions: new List<RolePermissionRequest> { new("User", action) }
        );
    }
}
