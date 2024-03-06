using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace LibraryApp.API.Users;

public class Role
{
    [BsonId]
    [BsonRepresentation(BsonType.ObjectId)]
    public string Id { get; set; }

    [BsonElement("name")]
    public string Name { get; set; }

    public override string ToString() => Name;
}