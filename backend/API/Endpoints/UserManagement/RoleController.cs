using System.Net.Mime;
using Application.UserManagement.Roles.DTOs;
using Application.UserManagement.Roles.Queries;
using LibraryApp.API.Extensions;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace LibraryApp.API.Controllers.UserManagement;

[Route("api/roles")]
[ApiExplorerSettings(GroupName = "User Management")]
public class RoleController(IMediator mediator) : Controller
{
    [HttpGet]
    [Produces(MediaTypeNames.Application.Json)]
    [ProducesResponseType(typeof(RoleDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetRoles([FromQuery] GetActiveRolesQuery query)
    {
        var roleResults = await mediator.Send(query);
        return roleResults.Match(
            Ok,
            errors =>
            {
                return NotFound(new ResultError(
                    Title: "Roles not found",
                    Details: string.Join($",{Environment.NewLine}", errors),
                    StatusCode: StatusCodes.Status404NotFound
                ));
            }
        );
    }
}
