using System.Net.Mime;
using Application.LoanManagement.BookCheckouts.DTOs;
using Application.LoanManagement.BookCheckouts.Queries;
using LibraryApp.API.Extension;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace LibraryApp.API.Controllers;

[Route("api/book-checkouts")]
public class BookCheckoutController(IMediator mediator) : Controller
{
    private readonly IMediator _mediator = mediator;

    [HttpGet]
    [Produces(MediaTypeNames.Application.Json)]
    [ProducesResponseType(typeof(BookCheckoutDto), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetCheckedOutBooks([FromQuery] GetCheckedOutBooksQuery query)
    {
        var bookResults = await _mediator.Send(query);
        return bookResults.Match(
            Ok,
            errors =>
            {
                var error = errors.FirstOrDefault();
                var resultError = new ResultError(
                    Title: "Checked Out Books Not Found",
                    Details: error?.Message,
                    StatusCode: StatusCodes.Status404NotFound
                );

                return NotFound(resultError);
            }
        );
    }
}
