namespace Infrastructure.Persistence.Context;

public class MongoDbConfiguration
{
    public required string ConnectionString { get; set; }
    public required string DatabaseName { get; set; }
    public required string UserCollectionName { get; set; }
    public required string RoleCollectionName { get; set; }
    public required string PermissionCollectionName { get; set; }
}