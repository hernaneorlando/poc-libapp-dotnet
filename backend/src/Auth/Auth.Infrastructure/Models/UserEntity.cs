namespace Auth.Infrastructure.Models;

using Auth.Domain.Enums;

/// <summary>
/// Relational entity for User aggregate persistence.
/// Flattens nested value objects for relational database storage.
/// Uses Data Mapper pattern with implicit operators.
/// </summary>
public sealed class UserEntity
{
    public Guid Id { get; set; }
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
    public int Version { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
    public bool IsActive { get; set; }

    /// <summary>
    /// Navigation properties for collections.
    /// </summary>
    public ICollection<UserRoleEntity> UserRoles { get; set; } = [];
    public ICollection<RefreshTokenEntity> RefreshTokens { get; set; } = [];

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
            Roles = [.. userEntity.UserRoles
                .Where(ur => ur.Role != null)
                .Select(ur => (Role)ur.Role!)],
            RefreshTokens = [.. userEntity.RefreshTokens.Select(rt => (RefreshToken)rt)],
            Version = userEntity.Version,
            CreatedAt = userEntity.CreatedAt,
            UpdatedAt = userEntity.UpdatedAt,
            IsActive = userEntity.IsActive
        };

        return user;
    }

    /// <summary>
    /// Converts domain aggregate root to relational entity (Domain → Entity).
    /// Flattens nested value objects into columns.
    /// </summary>
    public static implicit operator UserEntity(User user)
    {
        return new UserEntity
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
    }
}
