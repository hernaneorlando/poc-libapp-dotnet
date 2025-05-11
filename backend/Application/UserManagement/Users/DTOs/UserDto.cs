using Application.Common;
using Application.SeedWork.BaseDTO;
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

        userDto.ConvertModelBaseProperties(user);
        return userDto;
    }

    public static implicit operator User(UserDto userDto)
    {
        var user = new User(userDto.FirstName, userDto.LastName, userDto.Email, (Role)userDto.Role)
        {
            PasswordHash = userDto.PasswordHash,
            DocumentIdentification = userDto.DocumentIdentification
        };

        user.ConvertDtoBaseProperties(userDto);
        return user;
    }
}