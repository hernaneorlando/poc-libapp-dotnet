using Core.API;
using FluentAssertions;
using Xunit;

namespace Core.Tests;

/// <summary>
/// Tests for QueryStringWithFilters IParsable implementation
/// </summary>
public class QueryStringWithFiltersTests
{
    public record TestResponse { }

    #region Parse Method

    [Fact]
    public void Parse_EmptyString_ReturnsEmptyDictionary()
    {
        // Act
        var result = QueryStringWithFilters<ListTestQuery, TestResponse>.Parse("", null);

        // Assert
        result.Should().NotBeNull();
        result.Should().BeEmpty();
    }

    [Fact]
    public void Parse_SingleParameter_ReturnsDictionaryWithSingleEntry()
    {
        // Act
        var result = QueryStringWithFilters<ListTestQuery, TestResponse>.Parse("page=1", null);

        // Assert
        result.Should().ContainKey("page");
        result["page"].Should().HaveCount(1);
        result["page"][0].Should().Be("1");
    }

    [Fact]
    public void Parse_MultipleParameters_ReturnsDictionaryWithAllEntries()
    {
        // Act
        var result = QueryStringWithFilters<ListTestQuery, TestResponse>.Parse("page=1&size=10&order=Name", null);

        // Assert
        result.Should().HaveCount(3);
        result["page"][0].Should().Be("1");
        result["size"][0].Should().Be("10");
        result["order"][0].Should().Be("Name");
    }

    [Fact]
    public void Parse_DuplicateKeys_ReturnsDictionaryWithMultipleValues()
    {
        // Act
        var result = QueryStringWithFilters<ListTestQuery, TestResponse>.Parse("tag=important&tag=urgent&tag=review", null);

        // Assert
        result.Should().ContainKey("tag");
        result["tag"].Should().HaveCount(3);
        result["tag"].Should().Contain(new[] { "important", "urgent", "review" });
    }

    [Fact]
    public void Parse_UrlEncodedValues_DecodesCorrectly()
    {
        // Act
        var result = QueryStringWithFilters<ListTestQuery, TestResponse>.Parse("search=hello%20world&name=John%20Doe", null);

        // Assert
        result["search"][0].Should().Be("hello world");
        result["name"][0].Should().Be("John Doe");
    }

    [Fact]
    public void Parse_QueryStringStartsWithQuestion_IgnoresQuestion()
    {
        // Act
        var result = QueryStringWithFilters<ListTestQuery, TestResponse>.Parse("?page=2&size=20", null);

        // Assert
        result.Should().HaveCount(2);
        result["page"][0].Should().Be("2");
        result["size"][0].Should().Be("20");
    }

    [Fact]
    public void Parse_ParameterWithoutValue_CreatesEmptyValue()
    {
        // Act
        var result = QueryStringWithFilters<ListTestQuery, TestResponse>.Parse("page=1&empty=&size=10", null);

        // Assert
        result.Should().HaveCount(3);
        result["empty"].Should().HaveCount(1);
        result["empty"][0].Should().Be("");
    }

    [Fact]
    public void Parse_ComplexQueryString_ParsesCorrectly()
    {
        // Act
        var result = QueryStringWithFilters<ListTestQuery, TestResponse>.Parse(
            "_page=1&_size=10&_order=Name%20DESC&search=test&status=active&status=pending", null);

        // Assert
        result["_page"][0].Should().Be("1");
        result["_size"][0].Should().Be("10");
        result["_order"][0].Should().Be("Name DESC");
        result["search"][0].Should().Be("test");
        result["status"].Should().HaveCount(2);
        result["status"].Should().Contain(new[] { "active", "pending" });
    }

    #endregion

    #region TryParse Method

    [Fact]
    public void TryParse_NullString_ReturnsTrueWithEmptyDictionary()
    {
        // Act
        var success = QueryStringWithFilters<ListTestQuery, TestResponse>.TryParse(null, null, out var result);

        // Assert
        success.Should().BeTrue();
        result.Should().NotBeNull();
        result.Should().BeEmpty();
    }

    [Fact]
    public void TryParse_EmptyString_ReturnsTrueWithEmptyDictionary()
    {
        // Act
        var success = QueryStringWithFilters<ListTestQuery, TestResponse>.TryParse("", null, out var result);

        // Assert
        success.Should().BeTrue();
        result.Should().NotBeNull();
        result.Should().BeEmpty();
    }

    [Fact]
    public void TryParse_ValidQueryString_ReturnsTrueAndPopulates()
    {
        // Act
        var success = QueryStringWithFilters<ListTestQuery, TestResponse>.TryParse("page=1&size=10", null, out var result);

        // Assert
        success.Should().BeTrue();
        result.Should().NotBeNull();
        result!["page"][0].Should().Be("1");
        result["size"][0].Should().Be("10");
    }

    [Fact]
    public void TryParse_MultipleValuesForSameKey_ReturnsTrueAndGroupsValues()
    {
        // Act
        var success = QueryStringWithFilters<ListTestQuery, TestResponse>.TryParse(
            "category=books&category=magazines&category=articles", null, out var result);

        // Assert
        success.Should().BeTrue();
        result!["category"].Should().HaveCount(3);
    }

    [Fact]
    public void TryParse_UrlEncodedCharacters_DecodesCorrectly()
    {
        // Act
        var success = QueryStringWithFilters<ListTestQuery, TestResponse>.TryParse(
            "filter=price%3E100&description=special%20offer", null, out var result);

        // Assert
        success.Should().BeTrue();
        result!["filter"][0].Should().Be("price>100");
        result["description"][0].Should().Be("special offer");
    }

    [Fact]
    public void TryParse_TrailingAmpersand_IgnoresEmptyParameter()
    {
        // Act
        var success = QueryStringWithFilters<ListTestQuery, TestResponse>.TryParse("page=1&size=10&", null, out var result);

        // Assert
        success.Should().BeTrue();
        result.Should().HaveCount(2);
    }

    [Fact]
    public void TryParse_DoubleAmpersand_IgnoresEmptyParameter()
    {
        // Act
        var success = QueryStringWithFilters<ListTestQuery, TestResponse>.TryParse("page=1&&size=10", null, out var result);

        // Assert
        success.Should().BeTrue();
        result.Should().HaveCount(2);
    }

    #endregion

    #region GetQuery Integration

    [Fact]
    public void Parse_And_GetQuery_ProcessesOrderByCorrectly()
    {
        // Arrange
        var queryString = QueryStringWithFilters<ListTestQuery, TestResponse>.Parse("_order=Name&search=test", null);

        // Act
        var query = queryString.GetQuery();

        // Assert
        query.OrderBy.Should().Be("Name");
    }

    [Fact]
    public void Parse_And_GetQuery_ProcessesPaginationCorrectly()
    {
        // Arrange
        var queryString = QueryStringWithFilters<ListTestQuery, TestResponse>.Parse("_page=2&_size=25", null);

        // Act
        var query = queryString.GetQuery();

        // Assert
        query.PageNumber.Should().Be(2);
        query.PageSize.Should().Be(25);
    }

    [Fact]
    public void Parse_And_GetQuery_ProcessesFiltersCorrectly()
    {
        // Arrange
        var queryString = QueryStringWithFilters<ListTestQuery, TestResponse>.Parse("_page=1&_size=10&search=test&status=active", null);

        // Act
        var query = queryString.GetQuery();

        // Assert
        query.Filters.Should().HaveCount(2);
        query.Filters.Should().Contain(f => f.Field == "search");
        query.Filters.Should().Contain(f => f.Field == "status");
    }

    #endregion

    // Test query class
    public record ListTestQuery : BasePagedQuery<TestResponse> { }
}
