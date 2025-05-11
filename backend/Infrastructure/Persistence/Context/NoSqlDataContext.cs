using Infrastructure.Persistence.Entities.DocumentDb;
using Microsoft.Extensions.Options;
using MongoDB.Driver;

namespace Infrastructure.Persistence.Context;

public class NoSqlDataContext
{
    private readonly MongoClient mongoClient;
    private readonly IMongoDatabase database;

    public IMongoCollection<UserEntity> Users { get; set; }
    public IMongoCollection<RoleEntity> Roles { get; set; }
    public IMongoCollection<PermissionEntity> Permissions { get; set; }
    public IMongoCollection<AuditEntryEntity> AuditEntries { get; set; }

    public NoSqlDataContext(IOptions<MongoDbConfiguration> mongoDbConfiguration)
    {
        var config = mongoDbConfiguration.Value;
        mongoClient = new MongoClient(config.ConnectionString);

        database = mongoClient.GetDatabase(config.DatabaseName);

        Users = database.GetCollection<UserEntity>("users");
        Roles = database.GetCollection<RoleEntity>("roles");
        Permissions = database.GetCollection<PermissionEntity>("permissions");
        AuditEntries = database.GetCollection<AuditEntryEntity>("auditEntries");
    }
}
