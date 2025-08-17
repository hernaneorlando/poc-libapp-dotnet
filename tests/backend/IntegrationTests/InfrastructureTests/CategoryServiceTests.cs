using Application.CatalogManagement.Books.Services;
using Domain.CatalogManagement;
using Infrastructure.Persistence.Context;
using Infrastructure.Services.CatalogManagement;
using Microsoft.EntityFrameworkCore;

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
        var category = Category.Create("Fantasy", "Some fantasy book.").Value;

        // Act
        var newCategory = await _service.CreateCategoryAsync(category, CancellationToken.None);

        // Assert
        Assert.NotNull(newCategory);
        Assert.NotNull(newCategory.Value);
        var savedCategory = await _sqlContext.Categories.FindAsync(newCategory.Value.Id);
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