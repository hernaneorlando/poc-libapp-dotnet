using Auth.Application.Roles.DTOs;
using Auth.Application.Roles.Queries.ListRoles;
using Auth.Domain.Aggregates.Permission;
using Auth.Domain.Aggregates.Role;
using Auth.Domain.Enums;
using Auth.Infrastructure.Data;
using Auth.Infrastructure.Repositories;
using Auth.Infrastructure.Repositories.Interfaces;
using Common;
using Core.API;
using Core.Infrastructure;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace Auth.Tests.IntegrationTests.ApplicationHandlerTests.Roles;

public class ListRolesHandlerIntegrationTests : IAsyncLifetime
{
    private ServiceProvider _serviceProvider = null!;
    private AuthDbContext _dbContext = null!;
    private IMediator _mediator = null!;
    private IRoleRepository _roleRepository = null!;
    private ILogger<ListRolesQueryHandler> _logger = null!;

    public async Task InitializeAsync()
    {
        var services = new ServiceCollection();

        // Configure AuthDbContext with in-memory database
        services.AddDbContext<AuthDbContext>(options =>
            options.UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString()));

        // Add logging
        services.AddLogging(builder => builder.AddConsole());

        // Add repositories
        services.AddScoped<IRoleRepository, RoleRepository>();

        // Build services
        _serviceProvider = services.BuildServiceProvider();
        _dbContext = _serviceProvider.GetRequiredService<AuthDbContext>();
        _roleRepository = _serviceProvider.GetRequiredService<IRoleRepository>();
        _logger = _serviceProvider.GetRequiredService<ILogger<ListRolesQueryHandler>>();

        // Create a simple command dispatcher without MediatR licensing
        _mediator = new SimpleMediator<IRoleRepository, ListRolesQueryHandler, ListRolesQuery, PaginatedResponse<RoleDTO>>(_roleRepository, _logger);

        // Ensure database is created
        await _dbContext.Database.EnsureCreatedAsync();
    }

    #region Basic Listing

    [Fact]
    public async Task Handle_EmptyDatabase_ReturnsEmptyList()
    {
        // Arrange
        var query = new ListRolesQuery { PageNumber = 1, PageSize = 10 };

        // Act
        var result = await _mediator.Send(query);

        // Assert
        result.Should().NotBeNull();
        result.Data.Should().BeEmpty();
        result.TotalCount.Should().Be(0);
        result.TotalPages.Should().Be(0);
    }

    [Fact]
    public async Task Handle_SingleRole_ReturnsRole()
    {
        // Arrange
        var command = RoleTestDataBuilder.GenerateCreateRoleCommand(name: "Admin");
        var role = Role.Create(command.Name, command.Description);
        await _roleRepository.AddAsync(role);
        await _dbContext.SaveChangesAsync();

        var query = new ListRolesQuery { PageNumber = 1, PageSize = 10 };

        // Act
        var result = await _mediator.Send(query);

        // Assert
        result.Should().NotBeNull();
        result.Data.Should().HaveCount(1);
        result.TotalCount.Should().Be(1);
        result.TotalPages.Should().Be(1);
        result.Data!.First().Name.Should().Be("Admin");
    }

    [Fact]
    public async Task Handle_MultipleRoles_ReturnsAllRoles()
    {
        // Arrange
        var cmd1 = RoleTestDataBuilder.GenerateCreateRoleCommand();
        var cmd2 = RoleTestDataBuilder.GenerateCreateRoleCommand();
        var cmd3 = RoleTestDataBuilder.GenerateCreateRoleCommand();

        var role1 = Role.Create(cmd1.Name, cmd1.Description);
        var role2 = Role.Create(cmd2.Name, cmd2.Description);
        var role3 = Role.Create(cmd3.Name, cmd3.Description);

        await _roleRepository.AddAsync(role1);
        await _roleRepository.AddAsync(role2);
        await _roleRepository.AddAsync(role3);
        await _dbContext.SaveChangesAsync();

        var query = new ListRolesQuery { PageNumber = 1, PageSize = 10 };

        // Act
        var result = await _mediator.Send(query);

        // Assert
        result.Data.Should().HaveCount(3);
        result.TotalCount.Should().Be(3);
    }

    #endregion

    #region Pagination

    [Fact]
    public async Task Handle_FirstPage_ReturnsFirstPageItems()
    {
        // Arrange
        for (int i = 1; i <= 15; i++)
        {
            var cmd = RoleTestDataBuilder.GenerateCreateRoleCommand();
            var role = Role.Create(cmd.Name, cmd.Description);
            await _roleRepository.AddAsync(role);
        }
        await _dbContext.SaveChangesAsync();

        var query = new ListRolesQuery { PageNumber = 1, PageSize = 5 };

        // Act
        var result = await _mediator.Send(query);

        // Assert
        result.Data.Should().HaveCount(5);
        result.CurrentPage.Should().Be(1);
        result.TotalPages.Should().Be(3);
        result.TotalCount.Should().Be(15);
    }

    [Fact]
    public async Task Handle_SecondPage_ReturnsSecondPageItems()
    {
        // Arrange
        for (int i = 1; i <= 15; i++)
        {
            var cmd = RoleTestDataBuilder.GenerateCreateRoleCommand();
            var role = Role.Create(cmd.Name, cmd.Description);
            await _roleRepository.AddAsync(role);
        }
        await _dbContext.SaveChangesAsync();

        var query = new ListRolesQuery { PageNumber = 2, PageSize = 5 };

        // Act
        var result = await _mediator.Send(query);

        // Assert
        result.Data.Should().HaveCount(5);
        result.CurrentPage.Should().Be(2);
        result.TotalPages.Should().Be(3);
    }

    [Fact]
    public async Task Handle_LastPage_ReturnsRemainingItems()
    {
        // Arrange
        for (int i = 1; i <= 17; i++)
        {
            var cmd = RoleTestDataBuilder.GenerateCreateRoleCommand();
            var role = Role.Create(cmd.Name, cmd.Description);
            await _roleRepository.AddAsync(role);
        }
        await _dbContext.SaveChangesAsync();

        var query = new ListRolesQuery { PageNumber = 4, PageSize = 5 };

        // Act
        var result = await _mediator.Send(query);

        // Assert
        result.Data.Should().HaveCount(2);
        result.CurrentPage.Should().Be(4);
        result.TotalPages.Should().Be(4);
        result.TotalCount.Should().Be(17);
    }

    [Fact]
    public async Task Handle_DifferentPageSizes_ReturnsCorrectCounts()
    {
        // Arrange
        for (int i = 1; i <= 20; i++)
        {
            var cmd = RoleTestDataBuilder.GenerateCreateRoleCommand();
            var role = Role.Create(cmd.Name, cmd.Description);
            await _roleRepository.AddAsync(role);
        }
        await _dbContext.SaveChangesAsync();

        var query = new ListRolesQuery { PageNumber = 1, PageSize = 20 };

        // Act
        var result = await _mediator.Send(query);

        // Assert
        result.Data.Should().HaveCount(20);
        result.TotalPages.Should().Be(1);
    }

    [Fact]
    public async Task Handle_PageSizeOne_ReturnsSingleItemPerPage()
    {
        // Arrange
        for (int i = 1; i <= 5; i++)
        {
            var cmd = RoleTestDataBuilder.GenerateCreateRoleCommand();
            var role = Role.Create(cmd.Name, cmd.Description);
            await _roleRepository.AddAsync(role);
        }
        await _dbContext.SaveChangesAsync();

        var query = new ListRolesQuery { PageNumber = 1, PageSize = 1 };

        // Act
        var result = await _mediator.Send(query);

        // Assert
        result.Data.Should().HaveCount(1);
        result.TotalPages.Should().Be(5);
    }

    [Fact]
    public async Task Handle_PageSizeMax_ReturnsCorrectNumberOfItems()
    {
        // Arrange
        for (int i = 1; i <= 5; i++)
        {
            var cmd = RoleTestDataBuilder.GenerateCreateRoleCommand();
            var role = Role.Create(cmd.Name, cmd.Description);
            await _roleRepository.AddAsync(role);
        }
        await _dbContext.SaveChangesAsync();

        var query = new ListRolesQuery { PageNumber = 1, PageSize = 20 };

        // Act
        var result = await _mediator.Send(query);

        // Assert
        result.Data.Should().HaveCount(5);
        result.TotalPages.Should().Be(1);
    }

    #endregion

    #region Filtering

    [Fact]
    public async Task Handle_OnlyActiveRoles_ExcludesInactiveRoles()
    {
        // Arrange
        var cmd1 = RoleTestDataBuilder.GenerateCreateRoleCommand(name: "Active");
        var cmd2 = RoleTestDataBuilder.GenerateCreateRoleCommand(name: "Inactive");

        var activeRole = Role.Create(cmd1.Name, cmd1.Description);
        var inactiveRole = Role.Create(cmd2.Name, cmd2.Description);

        await _roleRepository.AddAsync(activeRole);
        await _roleRepository.AddAsync(inactiveRole);
        await _dbContext.SaveChangesAsync();

        // Deactivate one role
        inactiveRole.Deactivate();
        await _roleRepository.UpdateAsync(inactiveRole);
        await _dbContext.SaveChangesAsync();

        var query = new ListRolesQuery { PageNumber = 1, PageSize = 10 };

        // Act
        var result = await _mediator.Send(query);

        // Assert
        result.Data.Should().HaveCount(1);
        result.Data!.First().Name.Should().Be("Active");
    }

    [Fact]
    public async Task Handle_SearchByExactRoleName_ReturnsMatchingRole()
    {
        // Arrange
        var cmd1 = RoleTestDataBuilder.GenerateCreateRoleCommand(name: "Administrator", description: "Full system access");
        var cmd2 = RoleTestDataBuilder.GenerateCreateRoleCommand(name: "Editor", description: "Content editing access");
        var cmd3 = RoleTestDataBuilder.GenerateCreateRoleCommand(name: "Viewer", description: "Read-only access");

        var role1 = Role.Create(cmd1.Name, cmd1.Description);
        var role2 = Role.Create(cmd2.Name, cmd2.Description);
        var role3 = Role.Create(cmd3.Name, cmd3.Description);

        await _roleRepository.AddAsync(role1);
        await _roleRepository.AddAsync(role2);
        await _roleRepository.AddAsync(role3);
        await _dbContext.SaveChangesAsync();

        var query = new ListRolesQuery 
        { 
            PageNumber = 1, 
            PageSize = 10,
            Filters = 
            [
                FilterCriteria.Parse("Name", "Editor")
            ]
        };

        // Act
        var result = await _mediator.Send(query);

        // Assert
        result.Data.Should().HaveCount(1);
        result.Data!.First().Name.Should().Be("Editor");
    }

    [Fact]
    public async Task Handle_SearchByPartialRoleName_ReturnsMatchingRoles()
    {
        // Arrange
        var cmd1 = RoleTestDataBuilder.GenerateCreateRoleCommand(name: "SystemAdmin", description: "System administrator role");
        var cmd2 = RoleTestDataBuilder.GenerateCreateRoleCommand(name: "DataAdmin", description: "Data administrator role");
        var cmd3 = RoleTestDataBuilder.GenerateCreateRoleCommand(name: "Editor", description: "Editor role");

        var role1 = Role.Create(cmd1.Name, cmd1.Description);
        var role2 = Role.Create(cmd2.Name, cmd2.Description);
        var role3 = Role.Create(cmd3.Name, cmd3.Description);

        await _roleRepository.AddAsync(role1);
        await _roleRepository.AddAsync(role2);
        await _roleRepository.AddAsync(role3);
        await _dbContext.SaveChangesAsync();

        var query = new ListRolesQuery 
        { 
            PageNumber = 1, 
            PageSize = 10,
            Filters = 
            [
                FilterCriteria.Parse("Name", "*Admin*")
            ]
        };

        // Act
        var result = await _mediator.Send(query);

        // Assert
        result.Data.Should().HaveCount(2);
        result.Data.Should().Contain(r => r.Name == "SystemAdmin");
        result.Data.Should().Contain(r => r.Name == "DataAdmin");
    }

    [Fact]
    public async Task Handle_SearchByRoleNameStartsWith_ReturnsMatchingRoles()
    {
        // Arrange
        var cmd1 = RoleTestDataBuilder.GenerateCreateRoleCommand(name: "AdminPanel", description: "Admin panel access");
        var cmd2 = RoleTestDataBuilder.GenerateCreateRoleCommand(name: "AdminDashboard", description: "Admin dashboard access");
        var cmd3 = RoleTestDataBuilder.GenerateCreateRoleCommand(name: "UserViewer", description: "User viewer role");

        var role1 = Role.Create(cmd1.Name, cmd1.Description);
        var role2 = Role.Create(cmd2.Name, cmd2.Description);
        var role3 = Role.Create(cmd3.Name, cmd3.Description);

        await _roleRepository.AddAsync(role1);
        await _roleRepository.AddAsync(role2);
        await _roleRepository.AddAsync(role3);
        await _dbContext.SaveChangesAsync();

        var query = new ListRolesQuery 
        { 
            PageNumber = 1, 
            PageSize = 10,
            Filters = 
            [
                FilterCriteria.Parse("Name", "Admin*")
            ]
        };

        // Act
        var result = await _mediator.Send(query);

        // Assert
        result.Data.Should().HaveCount(2);
        result.Data.Should().AllSatisfy(r => r.Name.Should().StartWith("Admin"));
    }

    [Fact]
    public async Task Handle_SearchByRoleNameEndsWith_ReturnsMatchingRoles()
    {
        // Arrange
        var cmd1 = RoleTestDataBuilder.GenerateCreateRoleCommand(name: "SuperEditor", description: "Super editor role");
        var cmd2 = RoleTestDataBuilder.GenerateCreateRoleCommand(name: "BasicEditor", description: "Basic editor role");
        var cmd3 = RoleTestDataBuilder.GenerateCreateRoleCommand(name: "Viewer", description: "Viewer role");

        var role1 = Role.Create(cmd1.Name, cmd1.Description);
        var role2 = Role.Create(cmd2.Name, cmd2.Description);
        var role3 = Role.Create(cmd3.Name, cmd3.Description);

        await _roleRepository.AddAsync(role1);
        await _roleRepository.AddAsync(role2);
        await _roleRepository.AddAsync(role3);
        await _dbContext.SaveChangesAsync();

        var query = new ListRolesQuery 
        { 
            PageNumber = 1, 
            PageSize = 10,
            Filters = 
            [
                FilterCriteria.Parse("Name", "*Editor")
            ]
        };

        // Act
        var result = await _mediator.Send(query);

        // Assert
        result.Data.Should().HaveCount(2);
        result.Data.Should().AllSatisfy(r => r.Name.Should().EndWith("Editor"));
    }

    [Fact]
    public async Task Handle_SearchWithNoMatches_ReturnsEmptyList()
    {
        // Arrange
        var cmd1 = RoleTestDataBuilder.GenerateCreateRoleCommand(name: "Administrator", description: "Admin role");
        var cmd2 = RoleTestDataBuilder.GenerateCreateRoleCommand(name: "Editor", description: "Editor role");

        var role1 = Role.Create(cmd1.Name, cmd1.Description);
        var role2 = Role.Create(cmd2.Name, cmd2.Description);

        await _roleRepository.AddAsync(role1);
        await _roleRepository.AddAsync(role2);
        await _dbContext.SaveChangesAsync();

        var query = new ListRolesQuery 
        { 
            PageNumber = 1, 
            PageSize = 10,
            Filters = 
            [
                FilterCriteria.Parse("Name", "NonExistentRole")
            ]
        };

        // Act
        var result = await _mediator.Send(query);

        // Assert
        result.Data.Should().BeEmpty();
        result.TotalCount.Should().Be(0);
    }

    [Fact]
    public async Task Handle_SearchWithFilterAndPagination_ReturnsPaginatedResults()
    {
        // Arrange
        for (int i = 1; i <= 8; i++)
        {
            var name = $"Manager{i}";
            var description = $"Manager role for department {i}";
            var cmd = RoleTestDataBuilder.GenerateCreateRoleCommand(name: name, description: description);
            var role = Role.Create(cmd.Name, cmd.Description);
            await _roleRepository.AddAsync(role);
        }
        await _dbContext.SaveChangesAsync();

        var query = new ListRolesQuery 
        { 
            PageNumber = 1, 
            PageSize = 3,
            Filters = 
            [
                FilterCriteria.Parse("Name", "Manager*")
            ]
        };

        // Act
        var result = await _mediator.Send(query);

        // Assert
        result.Data.Should().HaveCount(3);
        result.TotalCount.Should().Be(8);
        result.TotalPages.Should().Be(3);
        result.Data.Should().AllSatisfy(r => r.Name.Should().StartWith("Manager"));
    }

    #endregion

    #region Sorting with Pagination

    [Fact]
    public async Task Handle_SortByNameAscending_ReturnsRolesSortedByNameAsc()
    {
        // Arrange
        var cmdZebra = RoleTestDataBuilder.GenerateCreateRoleCommand(name: "Zebra", description: "Zebra role description");
        var cmdAlpha = RoleTestDataBuilder.GenerateCreateRoleCommand(name: "Alpha", description: "Alpha role description");
        var cmdBeta = RoleTestDataBuilder.GenerateCreateRoleCommand(name: "Beta", description: "Beta role description");

        var roleZebra = Role.Create(cmdZebra.Name, cmdZebra.Description);
        var roleAlpha = Role.Create(cmdAlpha.Name, cmdAlpha.Description);
        var roleBeta = Role.Create(cmdBeta.Name, cmdBeta.Description);

        await _roleRepository.AddAsync(roleZebra);
        await _roleRepository.AddAsync(roleAlpha);
        await _roleRepository.AddAsync(roleBeta);
        await _dbContext.SaveChangesAsync();

        var query = new ListRolesQuery { PageNumber = 1, PageSize = 10, OrderBy = "Name" };

        // Act
        var result = await _mediator.Send(query);

        // Assert
        result.Data.Should().HaveCount(3);
        result.Data!.First().Name.Should().Be("Alpha");
        result.Data!.ElementAt(1).Name.Should().Be("Beta");
        result.Data!.ElementAt(2).Name.Should().Be("Zebra");
    }

    [Fact]
    public async Task Handle_SortByNameDescending_ReturnsRolesSortedByNameDesc()
    {
        // Arrange
        var cmdZebra = RoleTestDataBuilder.GenerateCreateRoleCommand(name: "Zebra", description: "Zebra role description");
        var cmdAlpha = RoleTestDataBuilder.GenerateCreateRoleCommand(name: "Alpha", description: "Alpha role description");
        var cmdBeta = RoleTestDataBuilder.GenerateCreateRoleCommand(name: "Beta", description: "Beta role description");

        var roleZebra = Role.Create(cmdZebra.Name, cmdZebra.Description);
        var roleAlpha = Role.Create(cmdAlpha.Name, cmdAlpha.Description);
        var roleBeta = Role.Create(cmdBeta.Name, cmdBeta.Description);

        await _roleRepository.AddAsync(roleZebra);
        await _roleRepository.AddAsync(roleAlpha);
        await _roleRepository.AddAsync(roleBeta);
        await _dbContext.SaveChangesAsync();

        var query = new ListRolesQuery { PageNumber = 1, PageSize = 10, OrderBy = "Name DESC" };

        // Act
        var result = await _mediator.Send(query);

        // Assert
        result.Data.Should().HaveCount(3);
        result.Data!.First().Name.Should().Be("Zebra");
        result.Data!.ElementAt(1).Name.Should().Be("Beta");
        result.Data!.ElementAt(2).Name.Should().Be("Alpha");
    }

    [Fact]
    public async Task Handle_SortByCreationDateAscending_ReturnsRolesSortedByCreatedAt()
    {
        // Arrange
        for (int i = 1; i <= 5; i++)
        {
            var cmd = RoleTestDataBuilder.GenerateCreateRoleCommand();
            var role = Role.Create(cmd.Name, cmd.Description);
            await _roleRepository.AddAsync(role);
            // Small delay to ensure different creation times
            await Task.Delay(10);
        }
        await _dbContext.SaveChangesAsync();

        var query = new ListRolesQuery { PageNumber = 1, PageSize = 10, OrderBy = "CreatedAt" };

        // Act
        var result = await _mediator.Send(query);

        // Assert
        result.Data.Should().HaveCount(5);
        // Verify they are in ascending order by checking first and last
        var firstDate = result.Data!.First().CreatedAt;
        var lastDate = result.Data!.Last().CreatedAt;
        (lastDate >= firstDate).Should().BeTrue();
    }

    [Fact]
    public async Task Handle_SortByNameWithPagination_ReturnsSortedPaginatedData()
    {
        // Arrange
        for (int i = 1; i <= 15; i++)
        {
            var name = $"Role{i:D2}";
            var cmd = RoleTestDataBuilder.GenerateCreateRoleCommand(name: name, description: $"Test role number {i}");
            var role = Role.Create(cmd.Name, cmd.Description);
            await _roleRepository.AddAsync(role);
        }
        await _dbContext.SaveChangesAsync();

        var query1 = new ListRolesQuery { PageNumber = 1, PageSize = 5, OrderBy = "Name" };
        var query2 = new ListRolesQuery { PageNumber = 2, PageSize = 5, OrderBy = "Name" };

        // Act
        var result1 = await _mediator.Send(query1);
        var result2 = await _mediator.Send(query2);

        // Assert
        result1.Data.Should().HaveCount(5);
        result2.Data.Should().HaveCount(5);
        
        // Verify first page items are in order
        result1.Data!.First().Name.Should().Be("Role01");
        result1.Data!.Last().Name.Should().Be("Role05");
        
        // Verify second page items are in order
        result2.Data!.First().Name.Should().Be("Role06");
        result2.Data!.Last().Name.Should().Be("Role10");
    }

    [Fact]
    public async Task Handle_SortingAcrossMultiplePages_MaintainsSortOrderConsistency()
    {
        // Arrange
        var names = new[] { "Zulu", "Charlie", "Bravo", "Delta", "Alpha", "Echo", "Foxtrot", "Golf" };
        foreach (var name in names)
        {
            var cmd = RoleTestDataBuilder.GenerateCreateRoleCommand(name: name, description: $"{name} role description");
            var role = Role.Create(cmd.Name, cmd.Description);
            await _roleRepository.AddAsync(role);
        }
        await _dbContext.SaveChangesAsync();

        var query1 = new ListRolesQuery { PageNumber = 1, PageSize = 3, OrderBy = "Name" };
        var query2 = new ListRolesQuery { PageNumber = 2, PageSize = 3, OrderBy = "Name" };
        var query3 = new ListRolesQuery { PageNumber = 3, PageSize = 3, OrderBy = "Name" };

        // Act
        var result1 = await _mediator.Send(query1);
        var result2 = await _mediator.Send(query2);
        var result3 = await _mediator.Send(query3);

        // Assert - Verify order is maintained across pages
        var allNames = result1.Data!
            .Concat(result2.Data!)
            .Concat(result3.Data!)
            .Select(r => r.Name)
            .ToList();

        allNames.Should().Equal(
            "Alpha", "Bravo", "Charlie",
            "Delta", "Echo", "Foxtrot",
            "Golf", "Zulu"
        );
    }

    #endregion

    #region Response Structure

    [Fact]
    public async Task Handle_Query_ReturnsValidPaginatedResponse()
    {
        // Arrange
        var cmd = RoleTestDataBuilder.GenerateCreateRoleCommand();
        var role = Role.Create(cmd.Name, cmd.Description);
        await _roleRepository.AddAsync(role);
        await _dbContext.SaveChangesAsync();

        var query = new ListRolesQuery { PageNumber = 1, PageSize = 10 };

        // Act
        var result = await _mediator.Send(query);

        // Assert
        result.Should().NotBeNull();
        result.Data.Should().NotBeNull();
        result.CurrentPage.Should().Be(1);
        result.TotalPages.Should().BeGreaterThanOrEqualTo(1);
        result.TotalCount.Should().BeGreaterThanOrEqualTo(1);
    }

    [Fact]
    public async Task Handle_Query_ReturnsDTOWithAllProperties()
    {
        // Arrange
        var cmd = RoleTestDataBuilder.GenerateCreateRoleCommand(name: "CompleteRole", description: "Complete role with all properties");
        var role = Role.Create(cmd.Name, cmd.Description);
        await _roleRepository.AddAsync(role);
        await _dbContext.SaveChangesAsync();

        var query = new ListRolesQuery { PageNumber = 1, PageSize = 10 };

        // Act
        var result = await _mediator.Send(query);

        // Assert
        var roleDto = result.Data!.First();
        roleDto.Id.Should().NotBeEmpty();
        roleDto.Name.Should().Be("CompleteRole");
        roleDto.Description.Should().Be("Complete role with all properties");
        roleDto.IsActive.Should().BeTrue();
    }

    [Fact]
    public async Task Handle_Query_ReturnsDTOWithPermissions()
    {
        // Arrange
        var cmd = RoleTestDataBuilder.GenerateCreateRoleCommandWithPermissions(
            ("User", "Create"),
            ("Book", "Read")
        );
        var role = Role.Create(cmd.Name, cmd.Description);
        foreach (var perm in cmd.Permissions)
        {
            var permission = new Permission(
                Enum.Parse<PermissionFeature>(perm.Feature),
                Enum.Parse<PermissionAction>(perm.Action)
            );
            role.AssignPermission(permission);
        }

        await _roleRepository.AddAsync(role);
        await _dbContext.SaveChangesAsync();

        var query = new ListRolesQuery { PageNumber = 1, PageSize = 10 };

        // Act
        var result = await _mediator.Send(query);

        // Assert
        var roleDto = result!.Data!.First();
        roleDto.Permissions.Should().HaveCount(2);
    }

    #endregion

    #region Edge Cases

    [Fact]
    public async Task Handle_QueryAfterRoleDeletion_ExcludesDeletedRole()
    {
        // Arrange
        var cmd1 = RoleTestDataBuilder.GenerateCreateRoleCommand(name: "Role1", description: "First role for deletion");
        var cmd2 = RoleTestDataBuilder.GenerateCreateRoleCommand(name: "Role2", description: "Second role to keep");

        var role1 = Role.Create(cmd1.Name, cmd1.Description);
        var role2 = Role.Create(cmd2.Name, cmd2.Description);

        await _roleRepository.AddAsync(role1);
        await _roleRepository.AddAsync(role2);
        await _dbContext.SaveChangesAsync();

        // Delete role1
        await _roleRepository.DeleteAsync(role1.Id);
        await _dbContext.SaveChangesAsync();

        var query = new ListRolesQuery { PageNumber = 1, PageSize = 10 };

        // Act
        var result = await _mediator.Send(query);

        // Assert
        result.Data.Should().HaveCount(1);
        result.Data!.First().Name.Should().Be("Role2");
    }

    [Fact]
    public async Task Handle_LargeDataset_ReturnsWithCorrectPagination()
    {
        // Arrange
        const int totalRoles = 250;
        for (int i = 1; i <= totalRoles; i++)
        {
            var cmd = RoleTestDataBuilder.GenerateCreateRoleCommand();
            var role = Role.Create(cmd.Name, cmd.Description);
            await _roleRepository.AddAsync(role);
        }
        await _dbContext.SaveChangesAsync();

        var query = new ListRolesQuery { PageNumber = 5, PageSize = 25 };

        // Act
        var result = await _mediator.Send(query);

        // Assert
        result.TotalCount.Should().Be(totalRoles);
        result.TotalPages.Should().Be(10);
        result.CurrentPage.Should().Be(5);
        result.Data.Should().HaveCount(25);
    }

    [Fact]
    public async Task Handle_RequestedPageExceedsTotalPages_ReturnsEmptyData()
    {
        // Arrange
        for (int i = 1; i <= 5; i++)
        {
            var cmd = RoleTestDataBuilder.GenerateCreateRoleCommand();
            var role = Role.Create(cmd.Name, cmd.Description);
            await _roleRepository.AddAsync(role);
        }
        await _dbContext.SaveChangesAsync();

        var query = new ListRolesQuery { PageNumber = 100, PageSize = 10 };

        // Act
        var result = await _mediator.Send(query);

        // Assert
        result.Data.Should().BeEmpty();
        result.TotalCount.Should().Be(5);
        result.TotalPages.Should().Be(1);
    }

    #endregion

    #region Consistency

    [Fact]
    public async Task Handle_ConsecutiveQueries_ReturnConsistentResults()
    {
        // Arrange
        var cmd = RoleTestDataBuilder.GenerateCreateRoleCommand(name: "ConsistentRole", description: "Consistent role");
        var role = Role.Create(cmd.Name, cmd.Description);
        await _roleRepository.AddAsync(role);
        await _dbContext.SaveChangesAsync();

        var query = new ListRolesQuery { PageNumber = 1, PageSize = 10 };

        // Act
        var result1 = await _mediator.Send(query);
        var result2 = await _mediator.Send(query);

        // Assert
        result1.Data.Should().HaveCount(result2.Data!.Count());
        result1.TotalCount.Should().Be(result2.TotalCount);
        result1.Data!.First().Id.Should().Be(result2.Data!.First().Id);
    }

    [Fact]
    public async Task Handle_AfterAddingRole_ReturnsUpdatedList()
    {
        // Arrange
        var query = new ListRolesQuery { PageNumber = 1, PageSize = 10 };

        var result1 = await _mediator.Send(query);

        // Add a new role
        var cmd = RoleTestDataBuilder.GenerateCreateRoleCommand(name: "NewRole", description: "New role added");
        var newRole = Role.Create(cmd.Name, cmd.Description);
        await _roleRepository.AddAsync(newRole);
        await _dbContext.SaveChangesAsync();

        // Act
        var result2 = await _mediator.Send(query);

        // Assert
        result1.TotalCount.Should().Be(0);
        result2.TotalCount.Should().Be(1);
        result2.Data!.First().Name.Should().Be("NewRole");
    }

    #endregion

    public async Task DisposeAsync()
    {
        if (_dbContext != null)
        {
            await _dbContext.DisposeAsync();
        }
        if (_serviceProvider != null)
        {
            await _serviceProvider.DisposeAsync();
        }
    }
}
