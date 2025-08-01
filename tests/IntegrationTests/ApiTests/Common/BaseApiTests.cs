using Infrastructure.Persistence.Context;
using LibraryApp.API;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Options;
using Mongo2Go;

namespace IntegrationTests.ApiTests.Common;

public abstract class BaseApiTests
    : IClassFixture<WebApplicationFactory<Program>>, IDisposable
{
    protected readonly WebApplicationFactory<Program> _webFactory;
    private readonly MongoDbRunner _mongoDbRunner;

    protected BaseApiTests(WebApplicationFactory<Program> factory)
    {
        _mongoDbRunner = MongoDbRunner.Start();
        _webFactory = factory.WithWebHostBuilder(builder =>
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

                services.RemoveAll<NoSqlDataContext>();
                services.AddSingleton(provider =>
                {
                    var mongoConfig = new MongoDbConfiguration
                    {
                        ConnectionString = _mongoDbRunner.ConnectionString,
                        DatabaseName = Guid.NewGuid().ToString()
                    };
                    
                    var options = Options.Create(mongoConfig);
                    return new NoSqlDataContext(options);
                });
            });
        });
    }

    public void Dispose()
    {
        _mongoDbRunner.Dispose();
        GC.SuppressFinalize(this);
    }
}
