using System.Net.Http.Json;
using Application.CatalogManagement.Books.DTOs;
using Application.CatalogManagement.Books.Services;
using Domain.CatalogManagement;
using Domain.CatalogManagement.ValueObjects;
using Infrastructure.Persistence.Context;
using Infrastructure.Services.CatalogManagement;
using LibraryApp.API;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace IntegrationTests.InfrastructureTests;

public class CategoryServiceTests
{
    private readonly SqlDataContext _sqlContext;
    private readonly ICategoryService _service;

    public CategoryServiceTests()
    {
        var options = new DbContextOptionsBuilder<SqlDataContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;

        _sqlContext = new SqlDataContext(options);
        _service = new CategoryService(_sqlContext);
    }
    
    [Fact]
    public async Task AddAsync_ValidCategory_SavesToDatabase()
    {
        // Arrange
        var category = new Category(new CategoryName("Fantasy"))
        {
            Description = "Some fantasy book."
        };

        // Act
        var result = await _service.CreateCategoryAsync(category, CancellationToken.None);

        // Assert
        Assert.True(result.IsSuccess);
        var savedCategory = await _sqlContext.Categories.FindAsync(result.Value.Id);
        Assert.NotNull(savedCategory);
        Assert.True(savedCategory.Active);
        Assert.True(savedCategory.ExternalId != default);
        Assert.Equal("Fantasy", savedCategory.Name);
        Assert.Equal("Some fantasy book.", savedCategory.Description);
        Assert.Empty(savedCategory.Books);
        Assert.True(savedCategory.CreatedAt != default);
        Assert.Null(savedCategory.UpdatedAt);
    }
}