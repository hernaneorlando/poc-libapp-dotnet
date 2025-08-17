using Bunit;
using LibraryApp.Web.Model;
using LibraryApp.Web.Model.CatalogManagement;
using LibraryApp.Web.Pages;
using LibraryApp.Web.Services.CatalogManagement;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using MudBlazor;

namespace Web.UnitTests.CatalogManagement;

public class CategoryOverviewTests : MudBlazorTestContext
{
    private readonly Mock<ICategoryService> categoryServiceMock;

    public CategoryOverviewTests()
    {
        categoryServiceMock = new Mock<ICategoryService>();
    }

    [Fact]
    public void CategoriesOverview_EmptyTable()
    {
        // Arrange
        Services.AddSingleton(categoryServiceMock.Object);
        categoryServiceMock
            .Setup(m => m.GetCategories(It.IsAny<PagedRequest>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new PagedResponseDTO<CategoryDto>());

        // Act
        var cut = RenderMudComponent<CategoriesOverview>();

        // Assert
        Assert.True(cut.Instance is not null);
        Assert.True(cut.HasComponent<MudTable<CategoryDto>>());

        var table = cut.FindComponent<MudTable<CategoryDto>>();
        Assert.Single(table.Instance.TableContext.HeaderRows);
        Assert.Empty(table.Instance.Context.Rows);
    }

    [Fact]
    public void CategoriesOverview_NonEmptyTable()
    {
        // Arrange
        Services.AddSingleton(categoryServiceMock.Object);
        var categories = new CategoryDto[]
        {
                new("SciFi", "Some SciFi description"),
                new("Fantasy", "Some Fantasy description"),
        };

        var response = new PagedResponseDTO<CategoryDto>
        {
            Data = categories,
            TotalRecords = categories.Length
        };

        categoryServiceMock
            .Setup(m => m.GetCategories(It.IsAny<PagedRequest>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(response);

        // Act
        var cut = RenderMudComponent<CategoriesOverview>();

        // Assert
        Assert.True(cut.Instance is not null);
        Assert.True(cut.HasComponent<MudTable<CategoryDto>>());

        var table = cut.FindComponent<MudTable<CategoryDto>>();
        Assert.Single(table.Instance.TableContext.HeaderRows);
        Assert.Equal(2, table.Instance.Context.Rows.Count);
        Assert.True(table.Instance.Context.Rows.All(x => categories.Contains(x.Key)));
    }
}
