namespace Catalog.Domain.ValueObjects;

/// <summary>
/// Value Object representing a category name.
/// Has business rules for validation.
/// </summary>
public sealed class CategoryName : ValueObject
{
    public const int MaxLength = 100;
    
    public string Value { get; }

    private CategoryName(string value) => Value = value;

    public static CategoryName Create(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
            throw new ArgumentException("Category name cannot be empty");

        if (value.Length > MaxLength)
            throw new ArgumentException($"Category name cannot exceed {MaxLength} characters");

        return new CategoryName(value.Trim());
    }

    protected override IEnumerable<object?> GetEqualityComponents()
    {
        yield return Value;
    }

    public static implicit operator string(CategoryName name) => name.Value;

    public override string ToString() => Value;
}
