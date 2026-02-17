using Microsoft.AspNetCore.Components;

namespace LibraryApp.Web.Components;

public partial class BookCard
{
    [Parameter]
    public int Id { get; set; }

    [Parameter]
    public string Title { get; set; } = string.Empty;

    [Parameter]
    public string Author { get; set; } = string.Empty;

    [Parameter]
    public string Category { get; set; } = string.Empty;

    [Parameter]
    public double Rating { get; set; } = 0;

    [Parameter]
    public DateTime DateAdded { get; set; }

    [Parameter]
    public string? ImageUrl { get; set; }

    [Parameter]
    public EventCallback OnViewDetails { get; set; }

    private async Task HandleViewDetails()
    {
        await OnViewDetails.InvokeAsync();
    }
}
