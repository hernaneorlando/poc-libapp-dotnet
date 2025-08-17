using System.Net.Mime;
using Application.UserManagement.Users.DTOs;
using Application.UserManagement.Users.Queries;
using LibraryApp.API.Extensions;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace LibraryApp.API.Controllers.UserManagement;

[Route("api/users")]
[ApiExplorerSettings(GroupName = "User Management")]
[Produces(MediaTypeNames.Application.Json)]
public class UserController(IMediator mediator) : Controller
{
    [HttpGet("{id}")]
    [ProducesResponseType(typeof(UserDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ResultError), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ResultError), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> GetUserById(string id)
    {
        var userResult = await mediator.Send(new GetUserByIdQuery { Id = id });
        return userResult.Match(
            Ok,
            errors =>
            {
                return NotFound(new ResultError(
                    Title: "User Not Found",
                    Details: string.Join($",{Environment.NewLine}", errors),
                    StatusCode: StatusCodes.Status404NotFound
                ));
            }
        );
    }

    [HttpGet]
    [ProducesResponseType(typeof(UserDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetUsers([FromQuery] GetActiveUsersQuery query)
    {
        var userResults = await mediator.Send(query);
        return userResults.Match(
            Ok,
            errors =>
            {
                return NotFound(new ResultError(
                    Title: "Users not found",
                    Details: string.Join($",{Environment.NewLine}", errors),
                    StatusCode: StatusCodes.Status404NotFound
                ));
            }
        );
    }
}