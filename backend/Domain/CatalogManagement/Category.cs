using Domain.CatalogManagement.ValueObjects;
using Domain.Common;

namespace Domain.CatalogManagement;

public class Category : RelationalDbBaseModel<Category>
{
    public CategoryName Name { get; private set; }
    public string Description { get; set; } = string.Empty;
    public IList<Book> Books { get; set; } = [];

    public Category()
    {
        Name = CategoryName.Create("Default").Value;
    }

    public Category(string name)
    {
        var categoryNameResult = CategoryName.Create(name);
        if (!categoryNameResult.IsSuccess)
            throw new ValidationException(categoryNameResult.Errors);

        Name = categoryNameResult.Value;
    }

    public static ValidationResult<Category> CreateCategory(string name, string description)
    {
        var result = ValidationResult.Create<Category>();
        if (string.IsNullOrWhiteSpace(name) && string.IsNullOrWhiteSpace(description))
        {
            result.AddError("At least one field must be set to be created.");
            return result;
        }

        var category = new Category();
        category.AddCategoryName(name, result);
        category.Description = description;

        category.ValidateModel(result);
        result.AddValue(category);
        return result;
    }

    public ValidationResult<Category> UpdateCategory(string externalId, string name, string description)
    {
        var result = ValidationResult.Create<Category>();

        if (string.IsNullOrWhiteSpace(name) && string.IsNullOrWhiteSpace(description))
        {
            result.AddError("At least one field must be set to be updated.");
            return result;
        }

        if (!string.IsNullOrWhiteSpace(name) && name != Name)
            AddCategoryName(name, result);

        ValidateExternalId(externalId, result);
        Description = description;
        UpdatedAt = DateTime.UtcNow;

        if (!result.IsSuccess)
            return result;

        result.AddValue(this);
        return ValidateModel(result);
    }

    public ValidationResult<Category> UpdateCategory(Category category)
    {
        var result = ValidationResult.Create<Category>();
        if (category is null)
        {
            result.AddError("Category cannot be null.");
            return result;
        }

        if (category.ExternalId != ExternalId)
        {
            result.AddError("Categories must have the same External Id");
            return result;
        }

        return UpdateCategory(category.ExternalId.ToString(), category.Name.Value, category.Description);
    }

    private ValidationResult<Category> ValidateModel(ValidationResult<Category>? result = null)
    {
        result ??= ValidationResult.Create<Category>();

        if (ExternalId == Guid.Empty)
            result.AddError("External Id must not be empty.");

        if (Description is not null && Description.Length > 200)
            result.AddError("Category Description must not exceed 200 characters.");

        return result;
    }

    private void AddCategoryName(string name, ValidationResult<Category> result)
    {
        var categoryNameResult = CategoryName.Create(name);
        if (categoryNameResult.IsSuccess)
            Name = categoryNameResult.Value;
        else
            result.AddErrors(categoryNameResult.Errors);
    }
}