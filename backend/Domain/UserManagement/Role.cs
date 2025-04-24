using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace Domain.UserManagement;

public class Role
{
    [BsonId]
    [BsonRepresentation(BsonType.ObjectId)]
    public string Id { get; set; } = string.Empty;

    [BsonElement("name")]
    public string Name { get; set; } = string.Empty;

    public override string ToString() => Name;
}