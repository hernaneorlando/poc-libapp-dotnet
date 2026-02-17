using LibraryApp.Web.Services.CatalogManagement;
using LibraryApp.Web.Services.Auth;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using LibraryApp.Web;

var builder = WebAssemblyHostBuilder.CreateDefault(args);
builder.RootComponents.Add<App>("#app");
builder.RootComponents.Add<HeadOutlet>("head::after");

// Design System (Tailwind CSS) já carregado via index.html
// sem dependências de bibliotecas de UI (MudBlazor removido)

// Authentication Services
builder.Services.AddScoped<ITokenStorageService, TokenStorageService>();
builder.Services.AddSingleton<IAuthStateService, AuthStateService>();
builder.Services.AddScoped<IAuthService, AuthService>();
builder.Services.AddScoped<AuthenticationHandler>();

// Business Services
builder.Services.AddTransient<ICategoryService, CategoryService>();
builder.Services.AddScoped<IBookService, BookService>();

// HTTP Client with Authentication
builder.Services.AddHttpClient("API", client => {
    client.BaseAddress = new Uri(builder.Configuration["APIServer:Url"]!);
    client.DefaultRequestHeaders.Add("Accept", "application/json");
})
.AddHttpMessageHandler<AuthenticationHandler>();

// Register HttpClient for IAuthService (without AuthenticationHandler to avoid circular dependency)
builder.Services.AddHttpClient<IAuthService, AuthService>(client => {
    client.BaseAddress = new Uri(builder.Configuration["APIServer:Url"]!);
    client.DefaultRequestHeaders.Add("Accept", "application/json");
});

await builder.Build().RunAsync();
