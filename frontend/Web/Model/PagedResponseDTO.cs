namespace LibraryApp.Web.Model;

public record class PagedResponseDTO<TResponse>
{
    public int CurrentPage { get; set; }
    public int TotalPages { get; set; }
    public int TotalCount { get; set; }
    public IReadOnlyList<TResponse> Data { get; set; } = [];
}