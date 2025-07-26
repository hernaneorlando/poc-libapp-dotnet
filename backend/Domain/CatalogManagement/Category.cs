using Domain.CatalogManagement.ValueObjects;
using Domain.Common;

namespace Domain.CatalogManagement;

public class Category : RelationalDbModel<Category>
{
    public CategoryName Name { get; private set; }
    public string Description { get; set; } = string.Empty;
    public IList<Book> Books { get; set; } = [];

    private Category()
    {
        Name = CategoryName.Create("Default").Value;
    }

    public static Category Create() => new();

    public static ValidationResult<Category> Create(string name, string description)
    {
        var result = ValidationResult.Create<Category>();
        if (string.IsNullOrWhiteSpace(name) && string.IsNullOrWhiteSpace(description))
        {
            result.AddError("At least one field must be set to be created");
            return result;
        }

        var category = new Category
        {
            Description = description
        };

        category.AddName(name, result);

        category.Validate(result);
        result.AddValue(category);
        return result;
    }

    public ValidationResult<Category> Update(Guid externalId, string name, string description)
    {
        var result = ValidationResult.Create<Category>();

        if (string.IsNullOrWhiteSpace(name) && string.IsNullOrWhiteSpace(description))
        {
            result.AddError("At least one field must be set to be updated");
            return result;
        }

        if (!string.IsNullOrWhiteSpace(name) && name != Name)
            AddName(name, result);

        if (ExternalId != externalId)
        {
            result.AddError("Categories must have the same External Id");
            return result;
        }

        Description = description;
        UpdatedAt = DateTime.UtcNow;

        Validate(result);
        result.AddValue(this);
        return result;
    }

    private ValidationResult<Category> Validate(ValidationResult<Category>? result = null)
    {
        result ??= ValidationResult.Create<Category>();

        if (Description is not null && Description.Length > 256)
            result.AddError("Category Description must not exceed 256 characters");

        return result;
    }

    private void AddName(string name, ValidationResult<Category> result)
    {
        var categoryNameResult = CategoryName.Create(name);
        if (categoryNameResult.IsSuccess)
            Name = categoryNameResult.Value;
        else
            result.AddErrors(categoryNameResult.Errors);
    }
}