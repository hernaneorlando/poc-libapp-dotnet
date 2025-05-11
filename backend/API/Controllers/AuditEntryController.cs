using Application.ReportManagement.AuditEntries.DTOs;
using Application.ReportManagement.AuditEntries.Queries;
using LibraryApp.API.Extension;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace LibraryApp.API.Controllers;

[Route("api/audit-entries")]
public class AuditEntryController(IMediator mediator) : Controller
{
    private readonly IMediator _mediator = mediator;

    [HttpGet]
    [ProducesResponseType(typeof(AuditEntryDTO), StatusCodes.Status200OK)]
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