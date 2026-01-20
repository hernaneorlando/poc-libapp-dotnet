namespace Catalog.Domain.DomainEvents;

using Catalog.Domain.Aggregates.Category;

/// <summary>
/// Domain event raised when a category is created.
/// </summary>
public sealed class CategoryCreatedEvent : DomainEvent
{
    public CategoryId CategoryId { get; }
    public string Name { get; }

    public CategoryCreatedEvent(CategoryId categoryId, string name)
    {
        CategoryId = categoryId;
        Name = name;
    }
}

/// <summary>
/// Domain event raised when a category is updated.
/// </summary>
public sealed class CategoryUpdatedEvent : DomainEvent
{
    public CategoryId CategoryId { get; }
    public string Name { get; }

    public CategoryUpdatedEvent(CategoryId categoryId, string name)
    {
        CategoryId = categoryId;
        Name = name;
    }
}

/// <summary>
/// Domain event raised when a category is deactivated.
/// </summary>
public sealed class CategoryDeactivatedEvent : DomainEvent
{
    public CategoryId CategoryId { get; }

    public CategoryDeactivatedEvent(CategoryId categoryId)
    {
        CategoryId = categoryId;
    }
}
