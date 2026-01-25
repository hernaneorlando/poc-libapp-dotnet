using Auth.Infrastructure.Repositories.Interfaces;
using Core.Infrastructure;

namespace Auth.Infrastructure.Repositories;

/// <summary>
/// Implementation of the User repository using Entity Framework Core.
/// Handles persistence operations for the User aggregate root.
/// Converts between relational entities and domain aggregates using implicit operators.
/// </summary>
public sealed class UserRepository(AuthDbContext context)
    : BaseRepository<User, UserId, UserEntity>(context), IUserRepository
{
    private readonly AuthDbContext _context = context;

    public async Task AddAsync(User user, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(user);

        var userEntity = (UserEntity)user;
        await _context.Users.AddAsync(userEntity, cancellationToken);

        // Add refresh tokens separately
        foreach (var token in user.RefreshTokens)
        {
            var tokenEntity = (RefreshTokenEntity)token;
            tokenEntity.UserId = user.Id.Value;
            tokenEntity.Id = Guid.NewGuid();
            tokenEntity.CreatedAt = DateTime.UtcNow;
            await _context.RefreshTokens.AddAsync(tokenEntity, cancellationToken);
        }

        // Add user roles separately
        foreach (var role in user.Roles)
        {
            var userRoleEntity = new UserRoleEntity
            {
                Id = Guid.NewGuid(),
                UserId = user.Id.Value,
                RoleId = role.Id.Value,
                AssignedAt = DateTime.UtcNow
            };
            await _context.UserRoles.AddAsync(userRoleEntity, cancellationToken);
        }
    }

    public async Task<User?> GetByIdAsync(UserId id, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(id);

        var userEntity = await _context.Users
            .Include(u => u.RefreshTokens)
            .Include(u => u.UserRoles)
                .ThenInclude(ur => ur.Role)
            .FirstOrDefaultAsync(u => u.Id == id.Value && u.IsActive, cancellationToken);

        return userEntity is null ? null : (User)userEntity;
    }

    public async Task UpdateAsync(User user, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(user);

        var userEntity = await _context.Users
            .Include(u => u.RefreshTokens)
            .Include(u => u.UserRoles)
            .FirstOrDefaultAsync(u => u.Id == user.Id.Value, cancellationToken) 
            ?? throw new InvalidOperationException($"User with ID {user.Id} not found");

        // Update basic properties
        userEntity.FirstName = user.FirstName;
        userEntity.LastName = user.LastName;
        userEntity.Username = user.Username.Value;
        userEntity.PasswordHash = user.PasswordHash;
        userEntity.Email = user.Contact.Email;
        userEntity.PhoneNumber = user.Contact.PhoneNumber;
        userEntity.AddressStreet = user.Contact.Address?.Street;
        userEntity.AddressCity = user.Contact.Address?.City;
        userEntity.AddressState = user.Contact.Address?.State;
        userEntity.AddressCountry = user.Contact.Address?.Country;
        userEntity.AddressZipCode = user.Contact.Address?.ZipCode;
        userEntity.Version = user.Version;
        userEntity.UpdatedAt = user.UpdatedAt ?? DateTime.UtcNow;
        userEntity.IsActive = user.IsActive;

        // Update roles
        _context.UserRoles.RemoveRange(userEntity.UserRoles);
        foreach (var role in user.Roles)
        {
            userEntity.UserRoles.Add(new UserRoleEntity
            {
                Id = Guid.NewGuid(),
                UserId = user.Id.Value,
                RoleId = role.Id.Value,
                AssignedAt = DateTime.UtcNow
            });
        }

        // Update refresh tokens
        _context.RefreshTokens.RemoveRange(userEntity.RefreshTokens);
        foreach (var token in user.RefreshTokens)
        {
            var tokenEntity = (RefreshTokenEntity)token;
            tokenEntity.UserId = user.Id.Value;
            tokenEntity.Id = Guid.NewGuid();
            tokenEntity.CreatedAt = DateTime.UtcNow;
            userEntity.RefreshTokens.Add(tokenEntity);
        }

        _context.Users.Update(userEntity);
    }

    public async Task DeleteAsync(UserId id, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(id);

        var userEntity = await _context.Users.FirstOrDefaultAsync(u => u.Id == id.Value, cancellationToken);
        if (userEntity is not null)
        {
            userEntity.IsActive = false;
            userEntity.UpdatedAt = DateTime.UtcNow;
            _context.Users.Update(userEntity);
        }
    }

    protected override User MapToDomain(UserEntity entity) => (User)entity;
}
