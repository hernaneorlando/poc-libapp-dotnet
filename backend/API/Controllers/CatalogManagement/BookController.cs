using System.Net.Mime;
using Application.CatalogManagement.Books.DTOs;
using Application.CatalogManagement.Books.Queries;
using LibraryApp.API.Extension;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace LibraryApp.API.Controllers.CatalogManagement;

[Route("api/books")]
[ApiExplorerSettings(GroupName = "Catalog Management")]
[Produces(MediaTypeNames.Application.Json)]
public class BookController(IMediator mediator) : Controller
{
    [HttpGet]
    [ProducesResponseType(typeof(BookDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ResultError), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetActiveBooks([FromQuery] GetActiveBooksQuery query)
    {
        var bookResults = await mediator.Send(query);
        return bookResults.Match(
            Ok,
            errors =>
            {
                return NotFound(new ResultError(
                    Title: "Books not found",
                    Details: string.Join($",{Environment.NewLine}", errors),
                    StatusCode: StatusCodes.Status404NotFound
                ));
            }
        );
    }
}