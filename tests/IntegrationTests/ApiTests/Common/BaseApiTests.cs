using Infrastructure.Persistence.Context;
using LibraryApp.API;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace IntegrationTests.ApiTests.Common;

public abstract class BaseApiTests(WebApplicationFactory<Program> factory) 
    : IClassFixture<WebApplicationFactory<Program>>
{
    protected readonly WebApplicationFactory<Program> _webFactory = factory.WithWebHostBuilder(builder =>
        {
            builder.UseEnvironment("IntegrationTests");
            builder.ConfigureServices(services =>
            {
                services.RemoveAll<DbContextOptions<SqlDataContext>>();
                services.RemoveAll<SqlDataContext>();
                services.AddDbContext<SqlDataContext>(options =>
                {
                    options.UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString());
                }, ServiceLifetime.Singleton);
            });
        });
}
