using System.Net.Mime;
using Application.CatalogManagement.Contributors.DTOs;
using Application.CatalogManagement.Contributors.Queries;
using LibraryApp.API.Extension;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace LibraryApp.API.Controllers.CatalogManagement;

[Route("api/contributors")]
[ApiExplorerSettings(GroupName = "Catalog Management")]
[Produces(MediaTypeNames.Application.Json)]
public class ContributorController(IMediator mediator) : Controller
{
    [HttpGet]
    [ProducesResponseType(typeof(ContributorDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ResultError), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ResultError), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> GetActiveContributors([FromQuery] GetActiveContributorsQuery query)
    {
        var contributorResults = await mediator.Send(query);
        return contributorResults.Match(
            Ok,
            errors =>
            {
                var error = errors.FirstOrDefault();
                var resultError = new ResultError(
                    Title: "Contributors Not Found",
                    Details: error?.Message,
                    StatusCode: StatusCodes.Status404NotFound
                );

                return NotFound(resultError);
            }
        );
    }
}
