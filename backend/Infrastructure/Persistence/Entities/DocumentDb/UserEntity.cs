using Application.UserManagement.Roles.DTOs;
using Application.UserManagement.Users.DTOs;
using Domain.UserManagement;
using Domain.UserManagement.ValueObjects;
using Infrastructure.Common;
using Infrastructure.Persistence.Common;

namespace Infrastructure.Persistence.Entities.DocumentDb;

public class UserEntity : DocumentDbEntity
{
    public required string Username { get; set; }
    public string? PasswordHash { get; set; }
    public required string FirstName { get; set; }
    public required string LastName { get; set; }
    public string? DocumentIdentification { get; set; }
    public required ContactValueObject Contact { get; set; }
    public required RoleEntity Role { get; set; }

    public static implicit operator UserDto(UserEntity userEntity)
    {
        return new UserDto(
            userEntity.Username,
            userEntity.FirstName,
            userEntity.LastName,
            userEntity.Contact.Email,
            (RoleDto)userEntity.Role)
        {
            DocumentIdentification = userEntity.DocumentIdentification,
            Active = userEntity.Active,
            CreatedAt = userEntity.CreatedAt,
            UpdatedAt = userEntity.UpdatedAt,
        };
    }

    public static implicit operator User(UserEntity userEntity)
    {
        var model = new User(
            userEntity.FirstName,
            userEntity.LastName,
            (UserContact)userEntity.Contact,
            (Role)userEntity.Role)
        {
            PasswordHash = userEntity.PasswordHash,
            DocumentIdentification = userEntity.DocumentIdentification,
        };

        model.ConvertEntityBaseProperties(userEntity);
        return model;
    }

    public static implicit operator UserEntity(User user)
    {
        var entity = new UserEntity
        {
            Username = user.Username,
            PasswordHash = user.PasswordHash,
            FirstName = user.FirstName,
            LastName = user.LastName,
            DocumentIdentification = user.DocumentIdentification,
            Contact = (ContactValueObject)user.Contact,
            Role = (RoleEntity)user.Role,
        };

        entity.ConvertModelBaseProperties(user);
        return entity;
    }
}