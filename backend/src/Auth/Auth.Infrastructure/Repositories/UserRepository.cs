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
    }

    public async Task<User?> GetByIdAsync(UserId id, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(id);

        var userEntity = await _context.Users
            .AsNoTracking()
            .Include(u => u.RefreshTokens)
            .Include(u => u.UserRoles)
                .ThenInclude(ur => ur.Role)
            .FirstOrDefaultAsync(u => u.Id == id.Value && u.IsActive, cancellationToken);

        return userEntity is null ? null : (User)userEntity;
    }

    public async Task<User?> GetByExternalIdAsync(long externalId, CancellationToken cancellationToken = default)
    {
        var userEntity = await _context.Users
            .AsNoTracking()
            .Include(u => u.RefreshTokens)
            .Include(u => u.UserRoles)
                .ThenInclude(ur => ur.Role)
            .FirstOrDefaultAsync(u => u.ExternalId == externalId && u.IsActive, cancellationToken);

        return userEntity is null ? null : (User)userEntity;
    }

    public async Task UpdateAsync(User user, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(user);

        // Check if entity is already tracked with the same ID to avoid reload conflicts
        // (useful for domain event handlers that load and update the same entity consecutively)
        var trackedEntity = _context.Users.Local.FirstOrDefault(u => u.Id == user.Id.Value);
        
        var userEntity = trackedEntity ?? await _context.Users
            .AsNoTracking()
            .Include(u => u.RefreshTokens)
            .Include(u => u.UserRoles)
                .ThenInclude(ur => ur.Role)
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

        // Update roles - only remove/add if roles have changed
        var currentRoleIds = userEntity.UserRoles.Select(ur => ur.RoleId).ToHashSet();
        var newRoleIds = user.Roles.Select(r => r.Id.Value).ToHashSet();

        // Remove roles that are no longer assigned
        var rolesToRemove = userEntity.UserRoles.Where(ur => !newRoleIds.Contains(ur.RoleId)).ToList();
        foreach (var roleToRemove in rolesToRemove)
        {
            _context.UserRoles.Remove(roleToRemove);
        }

        // Add new roles
        foreach (var role in user.Roles)
        {
            if (!currentRoleIds.Contains(role.Id.Value))
            {
                // Ensure role is persisted if it's new
                var existingRole = await _context.Roles.FirstOrDefaultAsync(r => r.Id == role.Id.Value, cancellationToken);
                if (existingRole is null)
                {
                    var roleEntity = (RoleEntity)role;
                    // Detach if already tracked to avoid conflicts
                    var trackedRole = _context.Roles.Local.FirstOrDefault(r => r.Id == roleEntity.Id);
                    if (trackedRole is not null)
                        _context.Entry(trackedRole).State = Microsoft.EntityFrameworkCore.EntityState.Detached;

                    await _context.Roles.AddAsync(roleEntity, cancellationToken);
                }

                userEntity.UserRoles.Add(new UserRoleEntity
                {
                    Id = Guid.NewGuid(),
                    UserId = user.Id.Value,
                    RoleId = role.Id.Value,
                    AssignedAt = DateTime.UtcNow
                });
            }
        }

        // Synchronize refresh token changes (revocation status, expiration)
        // Since entities are now tracked, changes will be detected automatically
        foreach (var entityToken in userEntity.RefreshTokens)
        {
            var domainToken = user.RefreshTokens.FirstOrDefault(t => t.Token == entityToken.Token);
            if (domainToken is not null)
            {
                // Synchronize revocation and expiration state from domain aggregate
                entityToken.RevokedAt = domainToken.RevokedAt;
                entityToken.ExpiresAt = domainToken.ExpiresAt;
                entityToken.IsRememberMe = domainToken.IsRememberMe;
                // Mark the collection entry as modified so EF Core detects the change
                _context.Entry(entityToken).State = EntityState.Modified;
            }
        }

        // Add any new tokens created in domain aggregate that aren't in the database yet
        foreach (var domainToken in user.RefreshTokens)
        {
            var existingEntityToken = userEntity.RefreshTokens.FirstOrDefault(rt => rt.Token == domainToken.Token);
            if (existingEntityToken is null)
            {
                // New token from domain - add to database
                var newTokenEntity = new RefreshTokenEntity
                {
                    Id = Guid.NewGuid(),
                    UserId = user.Id.Value,
                    Token = domainToken.Token,
                    ExpiresAt = domainToken.ExpiresAt,
                    RevokedAt = domainToken.RevokedAt,
                    IsRememberMe = domainToken.IsRememberMe,
                    CreatedAt = DateTime.UtcNow
                };
                _context.RefreshTokens.Add(newTokenEntity);
                userEntity.RefreshTokens.Add(newTokenEntity);
            }
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
