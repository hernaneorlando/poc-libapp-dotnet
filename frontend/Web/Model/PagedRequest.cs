using System.Dynamic;

namespace LibraryApp.Web.Model;

public record class PagedRequest(int PageNumber = 1, int PageSize = 10)
{
    public string? OrderBy { get; set; }
    public bool? IsDescending { get; set; }
}
