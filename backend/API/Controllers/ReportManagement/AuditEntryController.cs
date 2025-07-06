using System.Net.Mime;
using Application.ReportManagement.AuditEntries.DTOs;
using Application.ReportManagement.AuditEntries.Queries;
using LibraryApp.API.Extension;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace LibraryApp.API.Controllers.ReportManagement;

[Route("api/audit-entries")]
[ApiExplorerSettings(GroupName = "Report Management")]
[Produces(MediaTypeNames.Application.Json)]
public class AuditEntryController(IMediator mediator) : Controller
{
    private readonly IMediator _mediator = mediator;

    [HttpGet]
    [ProducesResponseType(typeof(AuditEntryDTO), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ResultError), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ResultError), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> GetAuditEntries([FromQuery] GetAuditEntriesQuery query)
    {
        var auditEntryResults = await _mediator.Send(query);
        return auditEntryResults.Match(
            Ok,
            errors =>
            {
                var error = errors.FirstOrDefault();
                var resultError = new ResultError(
                    Title: "Audit Entries Not Found",
                    Details: error?.Message,
                    StatusCode: StatusCodes.Status404NotFound
                );

                return NotFound(resultError);
            }
        );
    }
}