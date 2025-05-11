using System.Net.Mime;
using Application.CatalogManagement.Contributors.DTOs;
using Application.CatalogManagement.Contributors.Queries;
using LibraryApp.API.Extension;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace LibraryApp.API.Controllers;

[Route("api/contributors")]
public class ContributorController(IMediator mediator) : Controller
{
    private readonly IMediator _mediator = mediator;

    [HttpGet]
    [Produces(MediaTypeNames.Application.Json)]
    [ProducesResponseType(typeof(ContributorDto), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetActiveContributors([FromQuery] GetActiveContributorsQuery query)
    {
        var contributorResults = await _mediator.Send(query);
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
