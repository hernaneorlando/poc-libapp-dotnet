using Auth.Application.Roles.Commands.CreateRole;
using Auth.Application.Common;
using Auth.Domain.Aggregates.Role;
using Auth.Domain;
using Auth.Domain.Repositories;
using Auth.Infrastructure.Data;
using Auth.Infrastructure.Repositories;
using FluentAssertions;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System.Reflection;
using Xunit;

namespace IntegrationTests.Auth.IntegrationTests.ApplicationHandlerTests.Roles;

/// <summary>
/// Integration tests for the CreateRoleCommandHandler.
/// Tests the Application layer (handler) with a real in-memory database.
/// Validates business logic and database persistence without HTTP overhead.
/// </summary>
public class CreateRoleHandlerIntegrationTests : IAsyncLifetime
{
    private ServiceProvider _serviceProvider = null!;
    private AuthDbContext _dbContext = null!;
    private IMediator _mediator = null!;
    private IRoleRepository _roleRepository = null!;
    private ILogger<CreateRoleCommandHandler> _logger = null!;
    private IUnitOfWork _unitOfWork = null!;

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
        services.AddScoped<IUnitOfWork, UnitOfWork>();

        // Build services
        _serviceProvider = services.BuildServiceProvider();
        _dbContext = _serviceProvider.GetRequiredService<AuthDbContext>();
        _roleRepository = _serviceProvider.GetRequiredService<IRoleRepository>();
        _logger = _serviceProvider.GetRequiredService<ILogger<CreateRoleCommandHandler>>();
        _unitOfWork = _serviceProvider.GetRequiredService<IUnitOfWork>();
        
        // Create a simple command dispatcher without MediatR licensing
        _mediator = new SimpleMediator(_roleRepository, _logger, _unitOfWork);

        // Ensure database is created
        await _dbContext.Database.EnsureCreatedAsync();
    }

    /// <summary>
    /// Simple mediator implementation to avoid MediatR licensing issues
    /// </summary>
    private class SimpleMediator : IMediator
    {
        private readonly IRoleRepository _roleRepository;
        private readonly ILogger<CreateRoleCommandHandler> _logger;
        private readonly IUnitOfWork _unitOfWork;

        public SimpleMediator(IRoleRepository roleRepository, ILogger<CreateRoleCommandHandler> logger, IUnitOfWork unitOfWork)
        {
            _roleRepository = roleRepository;
            _logger = logger;
            _unitOfWork = unitOfWork;
        }

        public async Task<TResponse> Send<TResponse>(IRequest<TResponse> request, CancellationToken cancellationToken = default)
        {
            // Handle CreateRoleCommand specifically
            if (request is CreateRoleCommand createRoleCommand)
            {
                var handler = new CreateRoleCommandHandler(_roleRepository, _logger, _unitOfWork);
                return (TResponse)(object)await handler.Handle(createRoleCommand, cancellationToken);
            }

            throw new NotSupportedException($"Command type {request.GetType().Name} is not supported");
        }

        public async Task Send<TRequest>(TRequest request, CancellationToken cancellationToken = default) where TRequest : IRequest
        {
            // Handle void request types
            if (request is IRequest unitRequest)
            {
                await Send(unitRequest, cancellationToken);
                return;
            }
            throw new NotSupportedException($"Request type {typeof(TRequest).Name} not supported");
        }

        public Task Publish<TNotification>(TNotification notification, CancellationToken cancellationToken = default) where TNotification : INotification
        {
            return Task.CompletedTask;
        }

        public Task<object?> Send(object request, CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }

        public IAsyncEnumerable<TResponse> CreateStream<TResponse>(IStreamRequest<TResponse> request, CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }

        public IAsyncEnumerable<object?> CreateStream(object request, CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }

        public Task Publish(object notification, CancellationToken cancellationToken = default)
        {
            return Task.CompletedTask;
        }
    }

    private static void RegisterMediatRManually(IServiceCollection services)
    {
        // This method is no longer used
    }

    #region Success Scenarios

    [Fact]
    public async Task Handle_ValidCreateRoleCommand_ReturnsSuccess()
    {
        // Arrange
        var command = RoleTestDataBuilder.GenerateCreateRoleCommand(
            name: "Administrator",
            description: "Full system access"
        );

        // Act
        var result = await _mediator.Send(command);

        // Assert
        result.Should().BeOfType<Result<RoleResponse>.Success>();
        var successResult = (Result<RoleResponse>.Success)result;
        successResult.Data.Id.Should().NotBeEmpty();
    }

    [Fact]
    public async Task Handle_ValidCreateRoleCommand_PersistsToDatabase()
    {
        // Arrange
        var command = RoleTestDataBuilder.GenerateCreateRoleCommand(
            name: "Editor",
            description: "Can edit content"
        );

        // Act
        var result = await _mediator.Send(command);

        // Assert
        result.Should().BeOfType<Result<RoleResponse>.Success>();
        var successResult = (Result<RoleResponse>.Success)result;
        
        var savedRole = await _dbContext.Roles.FindAsync(Guid.Parse(successResult.Data.Id));
        savedRole.Should().NotBeNull();
        savedRole!.Name.Should().Be("Editor");
        savedRole.Description.Should().Be("Can edit content");
    }

    [Fact]
    public async Task Handle_CommandWithPermissions_AssignsPermissionsCorrectly()
    {
        // Arrange
        var command = RoleTestDataBuilder.GenerateCreateRoleCommandWithPermissions(
            ("User", "Create"),
            ("User", "Read"),
            ("Book", "Update")
        );

        // Act
        var result = await _mediator.Send(command);

        // Assert
        result.Should().BeOfType<Result<RoleResponse>.Success>();
        var successResult = (Result<RoleResponse>.Success)result;
        
        successResult.Data.Permissions.Should().HaveCount(3);
    }

    [Fact]
    public async Task Handle_CommandWithoutPermissions_CreatesRoleWithoutPermissions()
    {
        // Arrange
        var command = RoleTestDataBuilder.GenerateCreateRoleCommandWithoutPermissions();

        // Act
        var result = await _mediator.Send(command);

        // Assert
        result.Should().BeOfType<Result<RoleResponse>.Success>();
        var successResult = (Result<RoleResponse>.Success)result;
        
        successResult.Data.Permissions.Should().BeEmpty();
    }

    [Fact]
    public async Task Handle_ValidCommand_GeneratesRoleId()
    {
        // Arrange
        var command = RoleTestDataBuilder.GenerateCreateRoleCommand();

        // Act
        var result = await _mediator.Send(command);

        // Assert
        result.Should().BeOfType<Result<RoleResponse>.Success>();
        var successResult = (Result<RoleResponse>.Success)result;
        successResult.Data.Id.Should().NotBeEmpty();
        Guid.TryParse(successResult.Data.Id, out _).Should().BeTrue();
    }

    #endregion

    #region Validation Error Scenarios

    [Fact]
    public async Task Handle_EmptyName_ReturnsValidationError()
    {
        // Arrange
        var command = RoleTestDataBuilder.GenerateCreateRoleCommandWithEmptyName();

        // Act
        var result = await _mediator.Send(command);

        // Assert
        result.Should().BeOfType<Result<RoleResponse>.ValidationError>();
        var validationError = (Result<RoleResponse>.ValidationError)result;
        validationError.Errors.Should().NotBeEmpty();
    }

    [Fact]
    public async Task Handle_ShortName_ReturnsValidationError()
    {
        // Arrange
        var command = RoleTestDataBuilder.GenerateCreateRoleCommandWithShortName();

        // Act
        var result = await _mediator.Send(command);

        // Assert
        result.Should().BeOfType<Result<RoleResponse>.ValidationError>();
    }

    [Fact]
    public async Task Handle_LongName_ReturnsValidationError()
    {
        // Arrange
        var command = RoleTestDataBuilder.GenerateCreateRoleCommandWithLongName();

        // Act
        var result = await _mediator.Send(command);

        // Assert
        result.Should().BeOfType<Result<RoleResponse>.ValidationError>();
    }

    [Fact]
    public async Task Handle_EmptyDescription_ReturnsValidationError()
    {
        // Arrange
        var command = RoleTestDataBuilder.GenerateCreateRoleCommandWithEmptyDescription();

        // Act
        var result = await _mediator.Send(command);

        // Assert
        result.Should().BeOfType<Result<RoleResponse>.ValidationError>();
    }

    [Fact]
    public async Task Handle_ShortDescription_ReturnsValidationError()
    {
        // Arrange
        var command = RoleTestDataBuilder.GenerateCreateRoleCommandWithShortDescription();

        // Act
        var result = await _mediator.Send(command);

        // Assert
        result.Should().BeOfType<Result<RoleResponse>.ValidationError>();
    }

    [Fact]
    public async Task Handle_LongDescription_ReturnsValidationError()
    {
        // Arrange
        var command = RoleTestDataBuilder.GenerateCreateRoleCommandWithLongDescription();

        // Act
        var result = await _mediator.Send(command);

        // Assert
        result.Should().BeOfType<Result<RoleResponse>.ValidationError>();
    }

    [Fact]
    public async Task Handle_InvalidPermissionFeature_ReturnsValidationError()
    {
        // Arrange
        var command = RoleTestDataBuilder.GenerateCreateRoleCommandWithInvalidFeature("InvalidFeature");

        // Act
        var result = await _mediator.Send(command);

        // Assert
        result.Should().BeOfType<Result<RoleResponse>.ValidationError>();
    }

    [Fact]
    public async Task Handle_InvalidPermissionAction_ReturnsValidationError()
    {
        // Arrange
        var command = RoleTestDataBuilder.GenerateCreateRoleCommandWithInvalidAction("InvalidAction");

        // Act
        var result = await _mediator.Send(command);

        // Assert
        result.Should().BeOfType<Result<RoleResponse>.ValidationError>();
    }

    #endregion

    #region Database Integrity

    [Fact]
    public async Task Handle_MultipleCommandsInSequence_AllPersistIndependently()
    {
        // Arrange
        var command1 = RoleTestDataBuilder.GenerateCreateRoleCommand(
            name: "Role1",
            description: "First role"
        );
        var command2 = RoleTestDataBuilder.GenerateCreateRoleCommand(
            name: "Role2",
            description: "Second role"
        );

        // Act
        var result1 = await _mediator.Send(command1);
        var result2 = await _mediator.Send(command2);

        // Assert
        result1.Should().BeOfType<Result<RoleResponse>.Success>();
        result2.Should().BeOfType<Result<RoleResponse>.Success>();
        
        var roles = await _dbContext.Roles.ToListAsync();
        roles.Should().HaveCount(2);
    }

    [Fact]
    public async Task Handle_RoleCreation_SetsCreatedAtTimestamp()
    {
        // Arrange
        var beforeCreation = DateTime.UtcNow;
        var command = RoleTestDataBuilder.GenerateCreateRoleCommand();

        // Act
        var result = await _mediator.Send(command);
        var afterCreation = DateTime.UtcNow;

        // Assert
        result.Should().BeOfType<Result<RoleResponse>.Success>();
        var successResult = (Result<RoleResponse>.Success)result;
        
        var savedRole = await _dbContext.Roles.FindAsync(Guid.Parse(successResult.Data.Id));
        savedRole!.CreatedAt.Should().BeOnOrAfter(beforeCreation);
        savedRole.CreatedAt.Should().BeOnOrBefore(afterCreation);
    }

    [Fact]
    public async Task Handle_RoleCreation_CreatedRoleIsActive()
    {
        // Arrange
        var command = RoleTestDataBuilder.GenerateCreateRoleCommand();

        // Act
        var result = await _mediator.Send(command);

        // Assert
        result.Should().BeOfType<Result<RoleResponse>.Success>();
        var successResult = (Result<RoleResponse>.Success)result;
        
        var savedRole = await _dbContext.Roles.FindAsync(Guid.Parse(successResult.Data.Id));
        savedRole!.IsActive.Should().BeTrue();
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
