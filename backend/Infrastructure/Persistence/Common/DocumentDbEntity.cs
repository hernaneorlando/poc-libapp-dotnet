using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace Infrastructure.Persistence.Common;

public abstract class DocumentDbEntity : IAuditableEntity
{
    [BsonId]
    [BsonRepresentation(BsonType.ObjectId)]
    public string Id { get; set; } = string.Empty;

    [BsonGuidRepresentation(GuidRepresentation.Standard)]
    public Guid ExternalId { get; set; } = Guid.NewGuid();

    [BsonDateTimeOptions(Kind = DateTimeKind.Utc)]
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    [BsonDateTimeOptions(Kind = DateTimeKind.Utc)]
    [BsonIgnoreIfDefault]
    [BsonIgnoreIfNull]
    public DateTime? UpdatedAt { get; set; }

    [BsonRepresentation(BsonType.Boolean)]
    public bool Active { get; set; } = true;
}