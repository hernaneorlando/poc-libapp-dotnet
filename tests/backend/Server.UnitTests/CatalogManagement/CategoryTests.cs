using Domain.CatalogManagement;

namespace Server.UnitTests.CatalogManagement;

public class CategoryTests
{
    private const string StringEmpty = "";
    private const string WhiteSpace = " ";

    [Theory]
    [InlineData("Books about science fiction.")]
    [InlineData(StringEmpty)]
    [InlineData(null)]
    public void CreateCategory_ValidInputs_ReturnsCategory(string? description)
    {
        // Arrange
        const string validName = "Science Fiction";

        // Act
        var result = Category.Create(validName, description!);

        // Assert
        Assert.True(result.IsSuccess);
        Assert.NotNull(result.Value);
        Assert.Equal(validName, result.Value.Name.Value);
        Assert.Equal(description, result.Value.Description);
    }

    [Theory]
    [InlineData(null, null)]
    [InlineData(StringEmpty, StringEmpty)]
    [InlineData(WhiteSpace, WhiteSpace)]
    public void CreateCategory_AtLeastOneInput_ReturnsError(string? invalidName, string? invalidDescription)
    {
        // Arrange
        // Act
        var result = Category.Create(invalidName!, invalidDescription!);

        // Assert
        Assert.True(result.IsFailure);
        Assert.Contains("At least one field must be set to be created", result.Errors);
    }

    [Theory]
    [InlineData(null)]
    [InlineData(StringEmpty)]
    [InlineData(WhiteSpace)]
    public void CreateCategory_InvalidName_ReturnsError(string? invalidName)
    {
        // Act
        var result = Category.Create(invalidName!, "A description.");

        // Assert
        Assert.True(result.IsFailure);
        Assert.Contains("Category name cannot be empty", result.Errors);
    }

    [Fact]
    public void CreateCategory_NameTooLong_ReturnsError()
    {
        // Arrange
        var invalidName = "This is a very long category name that exceeds the maximum allowed length of fifty characters.";
        // Act
        var result = Category.Create(invalidName!, "A description.");

        // Assert
        Assert.True(result.IsFailure);
        Assert.Contains("Category Name must not exceed 50 characters", result.Errors);
    }

    [Fact]
    public void CreateCategory_DescriptionTooLong_ReturnsError()
    {
        // Arrange
        const string validName = "Science Fiction";
        string invalidDescription = new('a', 257);

        // Act
        var result = Category.Create(validName, invalidDescription);

        // Assert
        Assert.True(result.IsFailure);
        Assert.Contains("Category Description must not exceed 256 characters", result.Errors);
    }

    [Fact]
    public void UpdateCategory_ValidInputs_ReturnsUpdatedCategory()
    {
        // Arrange
        var categoryExternalId = Guid.NewGuid();
        var category = Category.Create("Science Fiction", "Books about science fiction.").Value;
        category.ExternalId = categoryExternalId;

        const string newName = "Fantasy";
        const string newDescription = "Books about fantasy.";

        // Act
        var result = category.Update(categoryExternalId, newName, newDescription);

        // Assert
        Assert.True(result.IsSuccess);
        Assert.Equal(newName, result.Value.Name.Value);
        Assert.Equal(newDescription, result.Value.Description);
    }

    [Theory]
    [InlineData(null, null)]
    [InlineData(StringEmpty, StringEmpty)]
    [InlineData(WhiteSpace, WhiteSpace)]
    public void UpdateCategory_AtLeastOneInput_ReturnsError(string? invalidName, string? invalidDescription)
    {
        // Arrange
        var category = Category.Create();

        // Act
        var result = category.Update(Guid.NewGuid(), invalidName!, invalidDescription!);

        // Assert
        Assert.True(result.IsFailure);
        Assert.Contains("At least one field must be set to be updated", result.Errors);
    }

    [Fact]
    public void UpdateCategory_NullName_ReturnsSuccess()
    {
        // Arrange
        var categoryExternalId = Guid.NewGuid();
        var newCategoryDescription = "Some new description";
        var category = Category.Create("Science Fiction", "Books about science fiction.").Value;
        category.ExternalId = categoryExternalId;

        // Act
        var result = category.Update(categoryExternalId, null!, newCategoryDescription);

        // Assert
        Assert.True(result.IsSuccess);
        Assert.Equal(category.Name, result.Value.Name.Value);
        Assert.Equal(category.Description, newCategoryDescription);
    }

    [Fact]
    public void UpdateCategory_SameName_ReturnsSuccess()
    {
        // Arrange
        var categoryExternalId = Guid.NewGuid();
        var newCategoryDescription = "Some new description";

        var category = Category.Create("Science Fiction", "Books about science fiction.").Value;
        category.ExternalId = categoryExternalId;

        // Act
        var result = category.Update(categoryExternalId, category.Name.Value, newCategoryDescription);

        // Assert
        Assert.True(result.IsSuccess);
        Assert.Equal(category.Name.Value, result.Value.Name.Value);
        Assert.Equal(category.Description, newCategoryDescription);
    }

    [Fact]
    public void UpdateCategory_InvalidInputs_ReturnsError()
    {
        // Arrange
        var category = Category.Create("Science Fiction", "Books about science fiction.").Value;
        category.ExternalId = Guid.NewGuid();

        // Act
        var result1 = category.Update(
            category.ExternalId,
            new('a', 51),
            "Some description");

        var result2 = category.Update(
            Guid.NewGuid(),
            null!,
            "Some description");

        // Assert
        Assert.True(result1.IsFailure);
        Assert.True(result2.IsFailure);
        Assert.Contains("Category Name must not exceed 50 characters", result1.Errors);
        Assert.Contains("Categories must have the same External Id", result2.Errors);
    }

    [Fact]
    public void DeactivateCategory_ValidExternalId_ReturnSuccess()
    {
        // Arrange
        var externalId = Guid.NewGuid();
        var category = Category.Create("Science Fiction", "Books about science fiction.").Value;
        category.ExternalId = externalId;
        category.Active = true;
        category.UpdatedAt = null;

        // Act
        var result = category.Deactivate(externalId.ToString());

        // Assert
        Assert.True(result.IsSuccess);

        var categoryResult = result.Value;
        Assert.False(categoryResult.Active);
        Assert.NotNull(categoryResult.UpdatedAt);
    }

    [Fact]
    public void DeactivateCategory_InvalidInputs_ReturnsError()
    {
        // Arrange
        var category = Category.Create("Science Fiction", "Books about science fiction.").Value;
        category.ExternalId = Guid.NewGuid();

        // Act
        var result1 = category.Deactivate(StringEmpty);
        var result2 = category.Deactivate("123456789");

        // Assert
        Assert.False(result1.IsSuccess);
        Assert.Contains("External Id must not be empty", result1.Errors);
        Assert.Contains("External Id must be a valid GUID", result2.Errors);
    }

    [Fact]
    public void DeactivateCategory_FromNullCategory_ReturnsError()
    {
        // Arrange
        var category = Category.Create("Science Fiction", "Books about science fiction.").Value;
        category.ExternalId = Guid.NewGuid();

        // Act
        var result = category.Deactivate(null!);

        // Assert
        Assert.True(result.IsFailure);
        Assert.Contains("Category cannot be null", result.Errors);
    }

    [Fact]
    public void DeactivateCategory_FromCategoryWithNotTheSameId_ReturnsError()
    {
        // Arrange
        var category = Category.Create("Science Fiction", "Books about science fiction.").Value;
        category.ExternalId = Guid.NewGuid();

        var categoryToDeactivate = Category.Create("Fantasy", "Books about fantasy.").Value;
        categoryToDeactivate.ExternalId = Guid.NewGuid();

        // Act
        var result = category.Deactivate(categoryToDeactivate);

        // Assert
        Assert.True(result.IsFailure);
        Assert.Contains("Category must have the same External Id", result.Errors);
    }

    [Fact]
    public void DeactivateCategory_FromCategoryAlreadyDeactivated_ReturnsError()
    {
        // Arrange
        var externalId = Guid.NewGuid();
        var category = Category.Create("Science Fiction", "Books about science fiction.").Value;
        category.ExternalId = externalId;
        category.Active = false;

        var categoryToDeactivate = Category.Create("Science Fiction", "Books about science fiction.").Value;
        categoryToDeactivate.ExternalId = externalId;

        // Act
        var result = category.Deactivate(categoryToDeactivate);

        // Assert
        Assert.True(result.IsFailure);
        Assert.Contains("Category is alredy archived", result.Errors);
    }

    [Fact]
    public void DeactivateCategory_FromValidCategory_ReturnsUpdatedCategory()
    {
        // Arrange
        var categoryExternalId = Guid.NewGuid();
        var updateAt = DateTime.UtcNow.AddDays(-10);
        var category = Category.Create("Science Fiction", "Books about science fiction.").Value;
        category.ExternalId = categoryExternalId;
        category.Active = true;
        category.UpdatedAt = updateAt;

        var categoryToDeactivate = Category.Create("Science Fiction", "Books about science fiction.").Value;
        categoryToDeactivate.ExternalId = categoryExternalId;

        // Act
        var result = category.Deactivate(categoryToDeactivate);

        // Assert
        Assert.True(result.IsSuccess);
        Assert.Equal(category.ExternalId, categoryToDeactivate.ExternalId);
        Assert.Equal(category.Name, categoryToDeactivate.Name);
        Assert.Equal(category.Description, categoryToDeactivate.Description);
        Assert.False(category.Active);
        Assert.True(category.UpdatedAt > updateAt);
    }
}
