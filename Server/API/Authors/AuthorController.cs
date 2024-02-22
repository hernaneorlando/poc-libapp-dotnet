using System.Net.Mime;
using Microsoft.AspNetCore.Mvc;

namespace LibraryApp.API.Authors;

[Route("author")]
public class AuthorController : Controller
{
    private readonly IAuthorService authorService;

    public AuthorController(IAuthorService authorService)
    {
        this.authorService = authorService;
    }

    [HttpGet]
    [Produces(MediaTypeNames.Application.Json)]
    [ProducesResponseType(typeof(AuthorDto), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetAll()
    {
        return Ok(await authorService.GetAll());
    }
}
