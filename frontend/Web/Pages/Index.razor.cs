using LibraryApp.Web.Services.CatalogManagement;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;

namespace LibraryApp.Web.Pages;

public partial class Index
{
    [Inject]
    public IBookService BookService { get; set; } = null!;

    private List<BookDto>? LatestBooks;
    private List<BookDto>? SearchResults;
    private string SearchQuery = string.Empty;
    private bool IsSearching = false;
    private bool HasSearched = false;

    protected override async Task OnInitializedAsync()
    {
        await LoadLatestBooks();
    }

    private async Task LoadLatestBooks()
    {
        LatestBooks = await BookService.GetLatestBooksAsync(5);
    }

    private async Task DoSearch()
    {
        if (string.IsNullOrWhiteSpace(SearchQuery))
        {
            ClearSearch();
            return;
        }

        IsSearching = true;
        HasSearched = true;
        SearchResults = null;

        // Simular delay mínimo
        await Task.Delay(300);

        SearchResults = await BookService.SearchBooksAsync(SearchQuery);
        IsSearching = false;
    }

    private async Task HandleSearch(KeyboardEventArgs e)
    {
        if (e.Code == "Enter" || e.Key == "Enter")
        {
            await DoSearch();
        }
    }

    private void ClearSearch()
    {
        SearchQuery = string.Empty;
        SearchResults = null;
        HasSearched = false;
    }

    private void HandleViewBook(BookDto book)
    {
        Console.WriteLine($"View book detail: {book.Title} by {book.Author}");
        // TODO: Navegar para página de detalhes do livro
        // Navigation.NavigateTo($"/books/{book.Id}");
    }
}
