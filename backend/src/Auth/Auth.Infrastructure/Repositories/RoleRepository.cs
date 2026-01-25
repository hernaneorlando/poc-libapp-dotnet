using Auth.Infrastructure.Repositories.Interfaces;
using Core.Infrastructure;

namespace Auth.Infrastructure.Repositories;

/// <summary>
/// Implementation of the Role repository using Entity Framework Core.
/// Handles persistence operations for the Role aggregate root.
/// Converts between relational entities and domain aggregates using implicit operators.
/// </summary>
public sealed class RoleRepository(AuthDbContext context) 
    : BaseRepository<Role, RoleId, RoleEntity>(context), IRoleRepository
{
    private readonly AuthDbContext _context = context;

    public async Task AddAsync(Role role, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(role);

        var roleEntity = (RoleEntity)role;
        await _context.Roles.AddAsync(roleEntity, cancellationToken);
    }

    public async Task<Role?> GetByIdAsync(RoleId id, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(id);

        var roleEntity = await _context.Roles
            .AsNoTracking()
            .FirstOrDefaultAsync(r => r.Id == id.Value && r.IsActive, cancellationToken);

        return roleEntity is null ? null : (Role)roleEntity;
    }

    public async Task UpdateAsync(Role role, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(role);

        var roleEntity = await _context.Roles
            .FirstOrDefaultAsync(r => r.Id == role.Id.Value, cancellationToken) 
            ?? throw new InvalidOperationException($"Role with ID {role.Id} not found");

        // Update basic properties
        roleEntity.Name = role.Name;
        roleEntity.Description = role.Description;
        roleEntity.Version = role.Version;
        roleEntity.UpdatedAt = role.UpdatedAt ?? DateTime.UtcNow;
        roleEntity.IsActive = role.IsActive;

        // Update permissions using implicit conversion to RoleEntity
        var updatedEntity = (RoleEntity)role;
        roleEntity.PermissionsJson = updatedEntity.PermissionsJson;

        _context.Roles.Update(roleEntity);
    }

    public async Task DeleteAsync(RoleId id, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(id);

        var roleEntity = await _context.Roles.FirstOrDefaultAsync(r => r.Id == id.Value, cancellationToken);
        if (roleEntity is not null)
        {
            roleEntity.IsActive = false;
            roleEntity.UpdatedAt = DateTime.UtcNow;
            _context.Roles.Update(roleEntity);
        }
    }

    protected override Role MapToDomain(RoleEntity entity) => (Role)entity;
}
