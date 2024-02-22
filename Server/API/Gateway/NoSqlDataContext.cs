using LibraryApp.API.Users;
using MongoDB.Driver;

namespace LibraryApp.API.Gateway;

public class NoSqlDataContext
{
    private readonly MongoClient mongoClient;
    private readonly IMongoDatabase database;

    public IMongoCollection<User> Users { get; set; }

    public NoSqlDataContext(MongoClientSettings mongoClientSettings)
    {
        mongoClient = new MongoClient(mongoClientSettings);
        database = mongoClient.GetDatabase("library");

        Users = database.GetCollection<User>("Users");
    }
}
