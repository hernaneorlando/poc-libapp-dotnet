using System.Net.Mime;
using Application.UserManagement.Permissions.Commands;
using Application.UserManagement.Permissions.DTOs;
using Application.UserManagement.Permissions.Queries;
using LibraryApp.API.Extension;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace LibraryApp.API.Controllers;

[Route("api/permissions")]
public class PermissionController(IMediator mediator) : Controller
{
    private readonly IMediator _mediator = mediator;

    [HttpGet]
    [Produces(MediaTypeNames.Application.Json)]
    [ProducesResponseType(typeof(PermissionDto), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetPermissions([FromQuery] GetActivePermissionsQuery query)
    {
        var permissionResults = await _mediator.Send(query);
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

    [HttpGet("{id}")]
    [Produces(MediaTypeNames.Application.Json)]
    [ProducesResponseType(typeof(PermissionDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetPermissionById(string id)
    {
        var query = new GetPermissionByIdQuery { Id = id };
        var permissionResult = await _mediator.Send(query);
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

    [HttpPost]
    [Produces(MediaTypeNames.Application.Json)]
    [ProducesResponseType(typeof(PermissionDto), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> CreatePermission([FromBody] CreatePermissionCommand command)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        var permissionResult = await _mediator.Send(command);
        return permissionResult.Match(
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
}