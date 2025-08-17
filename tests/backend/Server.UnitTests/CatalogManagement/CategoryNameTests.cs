using Domain.CatalogManagement.ValueObjects;

namespace Server.UnitTests.CatalogManagement;

public class CategoryNameTests
{
    [Fact]
    public void Create_ValidName_SetsValue()
    {
        // Arrange
        const string validName = "Science Fiction";

        // Act
        var categoryName = CategoryName.Create(validName);

        // Assert
        Assert.True(categoryName.IsSuccess);
        Assert.Empty(categoryName.Errors);
        Assert.Equal(validName, categoryName.Value);
    }

    [Theory]
    [InlineData(null, "Category name cannot be empty")]
    [InlineData("", "Category name cannot be empty")]
    [InlineData("  ", "Category name cannot be empty")]
    [InlineData("A very long category name that exceeds the maximum length", "Category Name must not exceed 50 characters")]
    public void Create_InvalidName_ReturnsFailure(string? emptyName, string expectedError)
    {
        // Act
        var categoryName = CategoryName.Create(emptyName!);

        // Assert
        Assert.False(categoryName.IsSuccess);
        Assert.Single(categoryName.Errors);
        Assert.Equal(expectedError, categoryName.Errors[0]);
    }

    [Fact]
    public void ImplicitConversion_ToString_ReturnsValue()
    {
        // Arrange
        const string name = "Fantasy";
        var categoryName = CategoryName.Create(name).Value;

        // Act
        string result = categoryName;

        // Assert
        Assert.Equal(name, result);
    }

    [Fact]
    public void Equals_SameValue_ReturnsTrue()
    {
        // Arrange
        var name1 = CategoryName.Create("Adventure").Value;
        var name2 = CategoryName.Create("Adventure").Value;

        // Act
        bool areEqualByMethod = name1.Equals(name2);
        bool areEqualByMethodReversed = name2.Equals(name1);
        bool areEqualByOperator = name1 == name2;
        bool areEqualByOperatorReversed = name2 == name1;
        bool areDifferentTypesEqual = name1.Equals("Adventure");
        bool areDifferentTypesEqualByOperator = name1 == (object)"Adventure";
        bool areDifferentByOperator = name1 != name2;

        // Assert
        Assert.True(areEqualByMethod);
        Assert.True(areEqualByMethodReversed);
        Assert.True(areEqualByOperator);
        Assert.True(areEqualByOperatorReversed);
        Assert.False(areDifferentTypesEqual);
        Assert.False(areDifferentTypesEqualByOperator);
        Assert.False(areDifferentByOperator);
    }

    [Fact]
    public void Equals_DifferentValue_ReturnsFalse()
    {
        // Arrange
        var name1 = CategoryName.Create("Adventure").Value;
        var name2 = CategoryName.Create("Fantasy").Value;

        // Act
        bool areEqualByMethod = name1.Equals(name2);
        bool areEqualByOperator = name1 == name2;
        bool areDifferentByOperator = name1 != name2;

        // Assert
        Assert.False(areEqualByMethod);
        Assert.False(areEqualByOperator);
        Assert.True(areDifferentByOperator);
    }

    [Fact]
    public void Equals_Null_ReturnsFalse()
    {
        // Arrange
        var name = CategoryName.Create("Adventure").Value;

        // Act
        bool areEqual = name.Equals(null);

        // Assert
        Assert.False(areEqual);
    }
}
