using Domain.ReportManagement;
using Domain.ReportManagement.Enums;
using Domain.ReportManagement.ValueObjects;
using Domain.UserManagement;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace Infrastructure.Persistence.Entities.DocumentDb;

public class AuditEntryEntity
{
    [BsonId]
    [BsonRepresentation(BsonType.ObjectId)]
    public required string Id { get; set; }

    [BsonGuidRepresentation(GuidRepresentation.Standard)]
    public Guid ExternalId { get; set; } = Guid.NewGuid();

    [BsonDateTimeOptions(Kind = DateTimeKind.Utc)]
    public required DateTime Timestamp { get; set; }

    public required string EntityName { get; set; }
    public required string EntityId { get; set; }
    public required AuditActionEnum Action { get; set; }

    [BsonRepresentation(BsonType.ObjectId)]
    public required string UserId { get; set; } = string.Empty;

    [BsonIgnore]
    public required UserEntity User { get; set; }

    public IList<FieldChange> Changes { get; set; } = [];

    public static implicit operator AuditEntry(AuditEntryEntity auditEntryEntity)
    {
        return new AuditEntry
        {
            Id = auditEntryEntity.Id,
            ExternalId = auditEntryEntity.ExternalId,
            Timestamp = auditEntryEntity.Timestamp,
            EntityName = auditEntryEntity.EntityName,
            EntityId = auditEntryEntity.EntityId,
            Action = auditEntryEntity.Action,
            User = (User)auditEntryEntity.User,
            Changes = auditEntryEntity.Changes,
        };
    }
    
    public static implicit operator AuditEntryEntity(AuditEntry auditEntry)
    {
        return new AuditEntryEntity
        {
            Id = auditEntry.Id,
            ExternalId = auditEntry.ExternalId,
            Timestamp = auditEntry.Timestamp,
            EntityName = auditEntry.EntityName,
            EntityId = auditEntry.EntityId,
            Action = auditEntry.Action,
            UserId = auditEntry.User.Id,
            User = (UserEntity)auditEntry.User,
            Changes = auditEntry.Changes
        };
    }
}