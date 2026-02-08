namespace Auth.Infrastructure.Models;

using Auth.Domain.Enums;
using Core.Infrastructure;

/// <summary>
/// Relational entity for User aggregate persistence.
/// Flattens nested value objects for relational database storage.
/// Uses Data Mapper pattern with implicit operators.
/// </summary>
public sealed class UserEntity : Entity
{
    public required string FirstName { get; set; }
    public required string LastName { get; set; }
    public required string Username { get; set; }
    public required string PasswordHash { get; set; }
    public required string Email { get; set; }
    public string? PhoneNumber { get; set; }
    public string? AddressStreet { get; set; }
    public string? AddressCity { get; set; }
    public string? AddressState { get; set; }
    public string? AddressZipCode { get; set; }
    public string? AddressCountry { get; set; }
    public int UserType { get; set; }

    /// <summary>
    /// Navigation properties for collections.
    /// </summary>
    public IList<UserRoleEntity> UserRoles { get; set; } = [];
    public IList<RefreshTokenEntity> RefreshTokens { get; set; } = [];

    /// <summary>
    /// Converts relational entity to domain aggregate root (Entity → Domain).
    /// Reconstructs flattened value objects directly.
    /// </summary>
    public static implicit operator User(UserEntity userEntity)
    {
        // Reconstruct nested value objects from flattened columns
        var address = !string.IsNullOrWhiteSpace(userEntity.AddressStreet)
            ? new Address
            {
                Street = userEntity.AddressStreet,
                City = userEntity.AddressCity ?? string.Empty,
                State = userEntity.AddressState ?? string.Empty,
                Country = userEntity.AddressCountry ?? string.Empty,
                ZipCode = userEntity.AddressZipCode
            }
            : null;

        var contact = new UserContact
        {
            Email = userEntity.Email,
            PhoneNumber = userEntity.PhoneNumber,
            Address = address
        };

        var username = !string.IsNullOrWhiteSpace(userEntity.Username)
            ? new Username { Value = userEntity.Username }
            : null;

        var user = new User
        {
            Id = UserId.From(userEntity.Id),
            FirstName = userEntity.FirstName,
            LastName = userEntity.LastName,
            Username = username ?? new Username { Value = string.Empty },
            PasswordHash = userEntity.PasswordHash,
            Contact = contact,
            UserType = (UserType)userEntity.UserType,
            Version = userEntity.Version,
            CreatedAt = userEntity.CreatedAt,
            UpdatedAt = userEntity.UpdatedAt,
            IsActive = userEntity.IsActive
        };

        foreach (var roleEntity in userEntity.UserRoles)
        {
            user.Roles.Add((Role)roleEntity.Role);
        }

        foreach (var tokenEntity in userEntity.RefreshTokens)
        {
            user.RefreshTokens.Add((RefreshToken)tokenEntity);
        }

        return user;
    }

    /// <summary>
    /// Converts domain aggregate root to relational entity (Domain → Entity).
    /// Flattens nested value objects into columns.
    /// </summary>
    public static implicit operator UserEntity(User user)
    {
        var userEntity = new UserEntity
        {
            Id = user.Id.Value,
            FirstName = user.FirstName,
            LastName = user.LastName,
            Username = user.Username.Value,
            PasswordHash = user.PasswordHash ?? string.Empty,
            Email = user.Contact.Email,
            PhoneNumber = user.Contact.PhoneNumber,
            AddressStreet = user.Contact.Address?.Street,
            AddressCity = user.Contact.Address?.City,
            AddressState = user.Contact.Address?.State,
            AddressZipCode = user.Contact.Address?.ZipCode,
            AddressCountry = user.Contact.Address?.Country,
            UserType = (int)user.UserType,
            Version = user.Version,
            CreatedAt = user.CreatedAt,
            UpdatedAt = user.UpdatedAt ?? DateTime.UtcNow,
            IsActive = user.IsActive
        };

        // Map refresh tokens
        foreach (var refreshToken in user.RefreshTokens)
        {
            userEntity.RefreshTokens.Add((RefreshTokenEntity)refreshToken);
        }

        // Map roles
        foreach (var role in user.Roles)
        {
            var roleEntity = (RoleEntity)role;
            userEntity.UserRoles.Add(new UserRoleEntity
            {
                Id = Guid.NewGuid(),
                UserId = user.Id.Value,
                User = userEntity,
                RoleId = role.Id.Value,
                Role = roleEntity,
                AssignedAt = DateTime.UtcNow
            });
        }

        return userEntity;
    }
}
