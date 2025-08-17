using System.Net.Mime;
using Application.CatalogManagement.Books.Commands;
using Application.CatalogManagement.Books.DTOs;
using Application.CatalogManagement.Books.Queries;
using LibraryApp.API.Extensions;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace LibraryApp.API.Controllers.CatalogManagement;

[Route("api/categories")]
[ApiExplorerSettings(GroupName = "Catalog Management")]
[Produces(MediaTypeNames.Application.Json)]
public class CategoryController(IMediator mediator) : Controller
{
    [HttpPost]
    [ProducesResponseType(typeof(CategoryDto), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ResultError), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> CreateCategory([FromBody] CreateCategoryCommand command)
    {
        var result = await mediator.Send(command);
        return result.Match(
            category => CreatedAtAction(nameof(GetCategoryById), new { id = category.Id }, category),
            errors =>
            {
                return BadRequest(new ResultError(
                    Title: "Category creation failed",
                    Details: string.Join($",{Environment.NewLine}", errors),
                    StatusCode: StatusCodes.Status400BadRequest
                ));
            }
        );
    }

    [HttpGet("{id}")]
    [ProducesResponseType(typeof(CategoryDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ResultError), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetCategoryById(string id)
    {
        var categoryResult = await mediator.Send(new GetCategoryByIdQuery { Id = id });
        return categoryResult.Match(
            Ok,
            errors =>
            {
                return NotFound(new ResultError(
                    Title: "Category not found",
                    Details: string.Join($",{Environment.NewLine}", errors),
                    StatusCode: StatusCodes.Status404NotFound
                ));
            }
        );
    }

    [HttpGet]
    [ProducesResponseType(typeof(CategoryDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ResultError), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetCategories([FromQuery] GetActiveCategoriesQuery query)
    {
        var categoryResults = await mediator.Send(query);
        return categoryResults.Match(
            Ok,
            errors =>
            {
                return NotFound(new ResultError(
                    Title: "Categories not found",
                    Details: string.Join($",{Environment.NewLine}", errors),
                    StatusCode: StatusCodes.Status404NotFound
                ));
            }
        );
    }

    [HttpPut("{id}")]
    [ProducesResponseType(typeof(CategoryDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ResultError), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> UpdateCategory(string id, [FromBody] UpdateCategoryCommand command)
    {
        command.Id = id;
        var result = await mediator.Send(command);
        return result.Match(
            Ok,
            errors =>
            {
                return BadRequest(new ResultError(
                    Title: "Category update failed",
                    Details: string.Join($",{Environment.NewLine}", errors),
                    StatusCode: StatusCodes.Status400BadRequest
                ));
            }
        );
    }

    [HttpDelete("{id}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ResultError), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> DeleteCategory(string id)
    {
        var result = await mediator.Send(new DeleteCategoryCommand(id));
        return result.Match(
            NoContent,
            errors =>
            {
                return BadRequest(new ResultError(
                    Title: "Category deletion failed",
                    Details: string.Join($",{Environment.NewLine}", errors),
                    StatusCode: StatusCodes.Status400BadRequest
                ));
            }
        );
    }
}