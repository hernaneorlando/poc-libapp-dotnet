namespace LibraryApp.Web.Model;

public record class PagedResponseDTO<TResponse>
{
    public ICollection<TResponse> Data { get; set; } = [];
    public int TotalRecords { get; set; }
    public int TotalPages { get; set; }
    public int CurrentPage { get; set; }
    public int PageSize { get; set; }
}