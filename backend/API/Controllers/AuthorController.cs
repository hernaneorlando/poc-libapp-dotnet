using System.Net.Mime;
using Application.CatalogManagement.Authors.DTOs;
using Application.CatalogManagement.Authors.Services;
using Microsoft.AspNetCore.Mvc;

namespace LibraryApp.API.Controllers;

[Route("author")]
public class AuthorController(IAuthorService authorService) : Controller
{
    private readonly IAuthorService authorService = authorService;

    [HttpGet]
    [Produces(MediaTypeNames.Application.Json)]
    [ProducesResponseType(typeof(AuthorDto), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetAll()
    {
        return Ok(await authorService.GetAll());
    }
}
