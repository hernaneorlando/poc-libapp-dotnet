namespace Catalog.Domain.Aggregates.Category;

/// <summary>
/// Aggregate Root representing a Category for organizing books.
/// </summary>
public sealed class Category : AggregateRoot<CategoryId>
{
    public CategoryName Name { get; private set; } = null!;
    public string? Description { get; private set; }

    private Category() { }

    /// <summary>
    /// Creates a new category aggregate.
    /// </summary>
    public static Category Create(string name, string? description = null)
    {
        var categoryName = CategoryName.Create(name);

        var category = new Category
        {
            Id = CategoryId.New(),
            Name = categoryName,
            Description = description?.Trim()
        };

        category.RaiseDomainEvent(new CategoryCreatedEvent(category.Id, category.Name));

        return category;
    }

    /// <summary>
    /// Updates category information.
    /// </summary>
    public void Update(string name, string? description = null)
    {
        var categoryName = CategoryName.Create(name);
        
        Name = categoryName;
        Description = description?.Trim();
        UpdatedAt = DateTime.UtcNow;

        RaiseDomainEvent(new CategoryUpdatedEvent(Id, Name));
    }

    /// <summary>
    /// Deactivates the category (soft delete).
    /// </summary>
    public new void Deactivate()
    {
        base.Deactivate();
        RaiseDomainEvent(new CategoryDeactivatedEvent(Id));
    }
}
