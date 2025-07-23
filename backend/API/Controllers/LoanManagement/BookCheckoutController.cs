using System.Net.Mime;
using Application.LoanManagement.BookCheckouts.DTOs;
using Application.LoanManagement.BookCheckouts.Queries;
using LibraryApp.API.Extension;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace LibraryApp.API.Controllers.LoanManagement;

[Route("api/book-checkouts")]
[ApiExplorerSettings(GroupName = "Loan Management")]
[Produces(MediaTypeNames.Application.Json)]
public class BookCheckoutController(IMediator mediator) : Controller
{
    private readonly IMediator _mediator = mediator;

    [HttpGet]
    [ProducesResponseType(typeof(BookCheckoutDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ResultError), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ResultError), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> GetCheckedOutBooks([FromQuery] GetCheckedOutBooksQuery query)
    {
        var bookResults = await _mediator.Send(query);
        return bookResults.Match(
            Ok,
            errors =>
            {
                return NotFound(new ResultError(
                    Title: "Checked Out Books not found",
                    Details: string.Join($",{Environment.NewLine}", errors),
                    StatusCode: StatusCodes.Status404NotFound
                ));
            }
        );
    }
}
