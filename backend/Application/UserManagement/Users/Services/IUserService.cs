using Domain.UserManagement;

namespace Application.UserManagement.Users.Services;

public interface IUserService
{
    Task<IList<User>> GetAll();
}