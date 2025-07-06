using Domain.SeedWork;
using Domain.SeedWork.Common;

namespace Domain.CatalogManagement;

public class Category : RelationalDbBaseModel
{
    public string Name { get; private set; } = string.Empty;
    public string? Description { get; set; }
    public IList<Book> Books { get; set; } = [];

    public Category() { }

    public Category(string name) => Name = name;

    public Notification Create(string name, string? description)
    {
        FillModel(name, description);
        var notification = ValidateModel();

        if (string.IsNullOrWhiteSpace(name))
            notification.AddError("Category Name must not be empty.");

        return notification;
    }

    public Notification Update(string name, string description)
    {
        FillModel(name, description);
        var notification = new Notification();
        if (string.IsNullOrWhiteSpace(name) && string.IsNullOrWhiteSpace(description))
            notification.AddError("At least one field must be set to be updated.");

        return notification;
    }

    public Notification ValidateModel(Notification? notification = null)
    {
        notification ??= new Notification();

        if (ExternalId == Guid.Empty)
            notification.AddError("External Id must not be empty.");

        if (string.IsNullOrWhiteSpace(Name))
            notification.AddError("Category Name can not be empty.");

        if (Name.Length > 50)
            notification.AddError("Category Name must not exceed 50 characters.");

        if (Description is not null && Description.Length > 200)
            notification.AddError("Category Description must not exceed 200 characters.");

        return notification;
    }

    private void FillModel(string? name, string? description)
    {
        if (!string.IsNullOrWhiteSpace(name))
            Name = name;

        if (description is not null)
            Description = description;
    }
}