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
    [HttpPost]
    [ProducesResponseType(typeof(PermissionDto), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ResultError), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> CreatePermission([FromBody] CreatePermissionCommand command)
    {
        var result = await mediator.Send(command);
        return result.Match(
            permission => CreatedAtAction(nameof(GetPermissionById), new { id = permission.Id }, permission),
            errors =>
            {
                var error = errors.FirstOrDefault();
                var resultError = new ResultError(
                    Title: "Permission Creation Failed",
                    Details: error?.Message,
                    StatusCode: StatusCodes.Status400BadRequest
                );

                return BadRequest(resultError);
            }
        );
    }

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
                var error = errors.FirstOrDefault();
                var resultError = new ResultError(
                    Title: "Permission Not Found",
                    Details: error?.Message,
                    StatusCode: StatusCodes.Status404NotFound
                );

                return NotFound(resultError);
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
                var error = errors.FirstOrDefault();
                var resultError = new ResultError(
                    Title: "Permissions Not Found",
                    Details: error?.Message,
                    StatusCode: StatusCodes.Status404NotFound
                );

                return NotFound(resultError);
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
                var error = errors.FirstOrDefault();
                var resultError = new ResultError(
                    Title: "Permission Update Failed",
                    Details: error?.Message,
                    StatusCode: StatusCodes.Status400BadRequest
                );

                return BadRequest(resultError);
            }
        );
    }

    [HttpDelete("{id}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> DeletePermission(string id)
    {
        var result = await mediator.Send(new DeletePermissionCommand(id));
        return result.Match(
            NoContent,
            errors =>
            {
                var error = errors.FirstOrDefault();
                var resultError = new ResultError(
                    Title: "Permission Deletion Failed",
                    Details: error?.Message,
                    StatusCode: StatusCodes.Status400BadRequest
                );

                return BadRequest(resultError);
            }
        );
    }
}