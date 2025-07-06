using System.Net.Mime;
using Application.CatalogManagement.Publishers.DTOs;
using Application.CatalogManagement.Publishers.Queries;
using LibraryApp.API.Extension;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace LibraryApp.API.Controllers.CatalogManagement;

[Route("api/publishers")]
[ApiExplorerSettings(GroupName = "Catalog Management")]
[Produces(MediaTypeNames.Application.Json)]
public class PublisherController(IMediator mediator) : Controller
{
    [HttpGet]
    [ProducesResponseType(typeof(PublisherDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ResultError), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ResultError), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> GetActivePublishers([FromQuery] GetActivePublishersQuery query)
    {
        var publisherResults = await mediator.Send(query);
        return publisherResults.Match(
            Ok,
            errors =>
            {
                var error = errors.FirstOrDefault();
                var resultError = new ResultError(
                    Title: "Publishers Not Found",
                    Details: error?.Message,
                    StatusCode: StatusCodes.Status404NotFound
                );

                return NotFound(resultError);
            }
        );
    }
}
