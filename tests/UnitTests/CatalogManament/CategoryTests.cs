using Domain.CatalogManagement;
using Domain.Common;

namespace UnitTests.CatalogManament;

public class CategoryTests
{
    private const string StringEmpty = "";
    private const string WhiteSpace = " ";

    [Theory]
    [InlineData(null)]
    [InlineData(StringEmpty)]
    [InlineData(WhiteSpace)]
    public void Constructor_InvalidName_ThrowsDomainException(string? name)
    {
        // Act & Assert
        Assert.Throws<ValidationException>(() => new Category(name!));
    }

    [Fact]
    public void Constructor_ValidName_SetsValue()
    {
        // Arrange
        const string validName = "Science Fiction";

        // Act
        var category = new Category(validName);
        var categoryDefault = new Category();

        // Assert
        Assert.NotNull(category.Name);
        Assert.Equal(validName, category.Name.Value);
        Assert.NotNull(categoryDefault.Name);
        Assert.Equal("Default", categoryDefault.Name.Value);
    }

    [Theory]
    [InlineData("Books about science fiction.")]
    [InlineData(StringEmpty)]
    [InlineData(null)]
    public void CreateCategory_ValidInputs_ReturnsCategory(string? description)
    {
        // Arrange
        const string validName = "Science Fiction";

        // Act
        var result = Category.CreateCategory(validName, description!);

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
        var result = Category.CreateCategory(invalidName!, invalidDescription!);

        // Assert
        Assert.False(result.IsSuccess);
        Assert.Contains("At least one field must be set to be created.", result.Errors);
    }

    [Theory]
    [InlineData(null)]
    [InlineData(StringEmpty)]
    [InlineData(WhiteSpace)]
    public void CreateCategory_InvalidName_ReturnsError(string? invalidName)
    {
        // Act
        var result = Category.CreateCategory(invalidName!, "A description.");

        // Assert
        Assert.False(result.IsSuccess);
        Assert.Contains("Category name cannot be empty.", result.Errors);
    }

    [Fact]
    public void CreateCategory_NameTooLong_ReturnsError()
    {
        // Arrange
        var invalidName = "This is a very long category name that exceeds the maximum allowed length of fifty characters.";
        // Act
        var result = Category.CreateCategory(invalidName!, "A description.");

        // Assert
        Assert.False(result.IsSuccess);
        Assert.Contains("Category Name must not exceed 50 characters.", result.Errors);
    }

    [Fact]
    public void CreateCategory_DescriptionTooLong_ReturnsError()
    {
        // Arrange
        const string validName = "Science Fiction";
        string invalidDescription = new('a', 201);

        // Act
        var result = Category.CreateCategory(validName, invalidDescription);

        // Assert
        Assert.False(result.IsSuccess);
        Assert.Contains("Category Description must not exceed 200 characters.", result.Errors);
    }

    [Fact]
    public void UpdateCategory_ValidInputs_ReturnsUpdatedCategory()
    {
        // Arrange
        var categoryExternalId = Guid.NewGuid();
        var category = new Category("Science Fiction")
        {
            ExternalId = categoryExternalId,
            Description = "Books about science fiction."
        };

        const string newName = "Fantasy";
        const string newDescription = "Books about fantasy.";

        // Act
        var result = category.UpdateCategory(categoryExternalId.ToString(), newName, newDescription);

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
        var newGuid = Guid.NewGuid().ToString();
        var category = new Category();

        // Act
        var result = category.UpdateCategory(newGuid, invalidName!, invalidDescription!);

        // Assert
        Assert.False(result.IsSuccess);
        Assert.Contains("At least one field must be set to be updated.", result.Errors);
    }

    [Fact]
    public void UpdateCategory_NullName_ReturnsSuccess()
    {
        // Arrange
        var categoryExternalId = Guid.NewGuid();
        var newCategoryDescription = "Some new description";
        var category = new Category("Science Fiction")
        {
            ExternalId = categoryExternalId,
            Description = "Books about science fiction."
        };

        // Act
        var result = category.UpdateCategory(categoryExternalId.ToString(), null!, newCategoryDescription);

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

        var category = new Category("Science Fiction")
        {
            ExternalId = categoryExternalId,
            Description = "Books about science fiction."
        };

        // Act
        var result = category.UpdateCategory(categoryExternalId.ToString(), category.Name.Value, newCategoryDescription);

        // Assert
        Assert.True(result.IsSuccess);
        Assert.Equal(category.Name.Value, result.Value.Name.Value);
        Assert.Equal(category.Description, newCategoryDescription);
    }

    [Fact]
    public void UpdateCategory_InvalidInputs_ReturnsError()
    {
        // Arrange
        var category = new Category("Science Fiction")
        {
            ExternalId = Guid.NewGuid(),
            Description = "Books about science fiction."
        };

        // Act
        var result1 = category.UpdateCategory(
            StringEmpty,
            new('a', 51),
            "Some description");

        var result2 = category.UpdateCategory(
            "123456789",
            null!,
            "Some description");

        // Assert
        Assert.False(result1.IsSuccess);
        Assert.False(result2.IsSuccess);
        Assert.Contains("External Id must not be empty.", result1.Errors);
        Assert.Contains("Category Name must not exceed 50 characters.", result1.Errors);
        Assert.Contains("External Id must be a valid GUID.", result2.Errors);
    }

    [Fact]
    public void UpdateCategory_FromNullCategory_ReturnsError()
    {
        // Arrange
        var category = new Category("Science Fiction")
        {
            ExternalId = Guid.NewGuid(),
            Description = "Books about science fiction."
        };

        // Act
        var result = category.UpdateCategory(null!);

        // Assert
        Assert.False(result.IsSuccess);
        Assert.Contains("Category cannot be null.", result.Errors);
    }

    [Fact]
    public void UpdateCategory_FromCategoryWithNotTheSameId_ReturnsError()
    {
        // Arrange
        var category = new Category("Science Fiction")
        {
            ExternalId = Guid.NewGuid(),
            Description = "Books about science fiction."
        };

        var changedCategory = new Category("Fantasy")
        {
            ExternalId = Guid.NewGuid(),
            Description = "Books about fantasy."
        };

        // Act
        var result = category.UpdateCategory(changedCategory);

        // Assert
        Assert.False(result.IsSuccess);
        Assert.Contains("Categories must have the same External Id", result.Errors);
    }

    [Fact]
    public void UpdateCategory_FromValidCategory_ReturnsUpdatedCategory()
    {
        // Arrange
        var categoryExternalId = Guid.NewGuid();
        var category = new Category("Science Fiction")
        {
            ExternalId = categoryExternalId,
            Description = "Books about science fiction."
        };

        const string newName = "Fantasy";
        const string newDescription = "Books about fantasy.";
        var changedCategory = new Category(newName)
        {
            ExternalId = categoryExternalId,
            Description = newDescription
        };

        // Act
        var result = category.UpdateCategory(changedCategory);

        // Assert
        Assert.True(result.IsSuccess);
        Assert.Equal(category.ExternalId, changedCategory.ExternalId);
        Assert.Equal(category.Name, changedCategory.Name);
        Assert.Equal(category.Description, changedCategory.Description);
        Assert.Equal(newName, category.Name);
        Assert.Equal(newDescription, category.Description);
    }

    [Fact]
    public void DeactivateCategory_ValidExternalId_ReturnSuccess()
    {
        // Arrange
        var externalId = Guid.NewGuid();
        var category = new Category("Science Fiction")
        {
            ExternalId = externalId,
            Description = "Books about science fiction.",
            Active = true,
            UpdatedAt = null
        };

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
        var category = new Category("Science Fiction")
        {
            ExternalId = Guid.NewGuid(),
            Description = "Books about science fiction."
        };

        // Act
        var result1 = category.Deactivate(StringEmpty);
        var result2 = category.Deactivate("123456789");

        // Assert
        Assert.False(result1.IsSuccess);
        Assert.Contains("External Id must not be empty.", result1.Errors);
        Assert.Contains("External Id must be a valid GUID.", result2.Errors);
    }

    [Fact]
    public void DeactivateCategory_FromNullCategory_ReturnsError()
    {
        // Arrange
        var category = new Category("Science Fiction")
        {
            ExternalId = Guid.NewGuid(),
            Description = "Books about science fiction."
        };

        // Act
        var result = category.Deactivate(null!);

        // Assert
        Assert.False(result.IsSuccess);
        Assert.Contains("Category cannot be null.", result.Errors);
    }

    [Fact]
    public void DeactivateCategory_FromCategoryWithNotTheSameId_ReturnsError()
    {
        // Arrange
        var category = new Category("Science Fiction")
        {
            ExternalId = Guid.NewGuid(),
            Description = "Books about science fiction."
        };

        var categoryToDeactivate = new Category("Fantasy")
        {
            ExternalId = Guid.NewGuid(),
            Description = "Books about fantasy."
        };

        // Act
        var result = category.Deactivate(categoryToDeactivate);

        // Assert
        Assert.False(result.IsSuccess);
        Assert.Contains("Category must have the same External Id.", result.Errors);
    }

    [Fact]
    public void DeactivateCategory_FromCategoryAlreadyDeactivated_ReturnsError()
    {
        // Arrange
        var externalId = Guid.NewGuid();
        var category = new Category("Science Fiction")
        {
            ExternalId = externalId,
            Description = "Books about science fiction.",
            Active = false,
        };

        var categoryToDeactivate = new Category("Science Fiction")
        {
            ExternalId = externalId,
            Description = "Books about science fiction."
        };

        // Act
        var result = category.Deactivate(categoryToDeactivate);

        // Assert
        Assert.False(result.IsSuccess);
        Assert.Contains("Category is alredy deactivated.", result.Errors);
    }

    [Fact]
    public void DeactivateCategory_FromValidCategory_ReturnsUpdatedCategory()
    {
        // Arrange
        var categoryExternalId = Guid.NewGuid();
        var updateAt = DateTime.UtcNow.AddDays(-10);
        var category = new Category("Science Fiction")
        {
            ExternalId = categoryExternalId,
            Description = "Books about science fiction.",
            Active = true,
            UpdatedAt = updateAt
        };

        var categoryToDeactivate = new Category("Science Fiction")
        {
            ExternalId = categoryExternalId,
            Description = "Books about science fiction."
        };

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
