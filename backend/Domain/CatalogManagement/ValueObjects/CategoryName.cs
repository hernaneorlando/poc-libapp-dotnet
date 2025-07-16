using Domain.Common;

namespace Domain.CatalogManagement.ValueObjects;

public sealed class CategoryName(string value) : ValueObject
{
    public string Value { get; } = value;

    public static ValidationResult<CategoryName> Create(string value)
    {
        var categoryName = new CategoryName(value);
        return categoryName.Validate();
    }

    public ValidationResult<CategoryName> Validate()
    {
        var result = ValidationResult.Create<CategoryName>();

        if (string.IsNullOrWhiteSpace(Value))
            result.AddError("Category name cannot be empty.");

        if (Value?.Length > 50)
            result.AddError("Category Name must not exceed 50 characters.");

        if (result.IsSuccess)
            result.AddValue(this);

        return result;
    }

    protected override IEnumerable<object> GetEqualityComponents()
    {
        yield return Value;
    }

    public static implicit operator string(CategoryName name) => name.Value;
}
