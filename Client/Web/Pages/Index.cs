using LibraryAppWeb.Web.Pages.Authors;
using Microsoft.AspNetCore.Components;

namespace LibraryAppWeb.Web.Pages;

public partial class Index : ComponentBase
{
    public IList<Author> authors = new List<Author>();

    protected override async Task OnInitializedAsync()
    {
        authors = await httpClient.GetFromJsonAsync<IList<Author>>("http://dotnet-server/author");
    }
}