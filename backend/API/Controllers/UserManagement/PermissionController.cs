using System.Net.Mime;
using Application.UserManagement.Permissions.Commands;
using Application.UserManagement.Permissions.DTOs;
using Application.UserManagement.Permissions.Queries;
using LibraryApp.API.Extension;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace LibraryApp.API.Controllers.UserManagement;

[Route("api/permissions")]
[ApiExplorerSettings(GroupName = "User Management")]
[Produces(MediaTypeNames.Application.Json)]
public class PermissionController(IMediator mediator) : Controller
{
    [HttpGet("{id}")]
    [ProducesResponseType(typeof(PermissionDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetPermissionById(string id)
    {
        var permissionResult = await mediator.Send(new GetPermissionByIdQuery { Id = id });
        return permissionResult.Match(
            Ok,
            errors =>
            {
                return NotFound(new ResultError(
                    Title: "Permission not found",
                    Details: string.Join($",{Environment.NewLine}", errors),
                    StatusCode: StatusCodes.Status404NotFound
                ));
            }
        );
    }

    [HttpGet]
    [ProducesResponseType(typeof(PermissionDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetPermissions([FromQuery] GetActivePermissionsQuery query)
    {
        var permissionResults = await mediator.Send(query);
        return permissionResults.Match(
            Ok,
            errors =>
            {
                return NotFound(new ResultError(
                    Title: "Permissions not found",
                    Details: string.Join($",{Environment.NewLine}", errors),
                    StatusCode: StatusCodes.Status404NotFound
                ));
            }
        );
    }

    [HttpPut("{id}")]
    [ProducesResponseType(typeof(PermissionDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> UpdatePermission(string id, [FromBody] UpdatePermissionCommand command)
    {
        command.Id = id;
        var result = await mediator.Send(command);
        return result.Match(
            Ok,
            errors =>
            {
                return BadRequest(new ResultError(
                    Title: "Permission update failed",
                    Details: string.Join($",{Environment.NewLine}", errors),
                    StatusCode: StatusCodes.Status400BadRequest
                ));
            }
        );
    }
}