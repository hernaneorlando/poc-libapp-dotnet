using LibraryAppWeb.Web.Pages.Authors;
using LibraryAppWeb.Web.Pages.Users;
using Microsoft.AspNetCore.Components;

namespace LibraryAppWeb.Web.Pages;

public partial class Index : ComponentBase
{
    public IList<Author> authors = new List<Author>();
    public IList<User> users = new List<User>();

    protected override async Task OnInitializedAsync()
    {
        authors = await httpClient.GetFromJsonAsync<IList<Author>>("http://dotnet-server:8080/api/contributors?PageNumber=1&PageSize=10");
        // users = await httpClient.GetFromJsonAsync<IList<User>>("http://dotnet-server:8080/users?PageNumber=1&PageSize=10");
    }
}