using System.Net.Mime;
using Application.UserManagement.Users.Services;
using Microsoft.AspNetCore.Mvc;

namespace LibraryApp.API.Users;

[Route("users")]
public class UserController(IUserService userService) : Controller
{
    private readonly IUserService userService = userService;

    [HttpGet]
    [Produces(MediaTypeNames.Application.Json)]
    public async Task<IActionResult> GetAll()
    {
        return Ok(await userService.GetAll());
    }
}
