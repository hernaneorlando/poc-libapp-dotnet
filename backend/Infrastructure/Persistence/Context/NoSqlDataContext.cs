using Domain.UserManagement;
using Microsoft.Extensions.Options;
using MongoDB.Driver;

namespace Infrastructure.Persistence.Context;

public class NoSqlDataContext
{
    private readonly MongoClient mongoClient;
    private readonly IMongoDatabase database;

    public IMongoCollection<User> Users { get; set; }

    public NoSqlDataContext(IOptions<MongoDbConfiguration> mongoDbConfiguration)
    {
        mongoClient = new MongoClient(mongoDbConfiguration.Value.ConnectionString);
        database = mongoClient.GetDatabase(mongoDbConfiguration.Value.DatabaseName);

        Users = database.GetCollection<User>(mongoDbConfiguration.Value.UserCollectionName);
    }
}
