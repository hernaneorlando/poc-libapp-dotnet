using System.Net.Mime;
using Application.CatalogManagement.Publishers.DTOs;
using Application.CatalogManagement.Publishers.Queries;
using LibraryApp.API.Extension;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace LibraryApp.API.Controllers;

[Route("api/publishers")]
public class PublisherController(IMediator mediator) : Controller
{
    private readonly IMediator _mediator = mediator;

    [HttpGet]
    [Produces(MediaTypeNames.Application.Json)]
    [ProducesResponseType(typeof(PublisherDto), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetActivePublishers([FromQuery] GetActivePublishersQuery query)
    {
        var publisherResults = await _mediator.Send(query);
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
