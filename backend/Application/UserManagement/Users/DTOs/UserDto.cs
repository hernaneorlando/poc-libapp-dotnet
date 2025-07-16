using Application.Common.BaseDTO;
using Application.UserManagement.Roles.DTOs;
using Domain.UserManagement;

namespace Application.UserManagement.Users.DTOs;

public record UserDto(string Username, string FirstName, string LastName, string Email, RoleDto Role) : BaseDto
{
    public string? PasswordHash { get; set; }
    public string? DocumentIdentification { get; set; }

    public static implicit operator UserDto(User user)
    {
        var userDto = new UserDto(user.Username, user.FirstName, user.LastName, user.Contact.Email, (RoleDto)user.Role)
        {
            PasswordHash = user.PasswordHash,
            DocumentIdentification = user.DocumentIdentification
        };

        userDto.ConvertBaseProperties(user);
        return userDto;
    }
}