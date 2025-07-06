using Domain.SeedWork.Common;
using Domain.SeedWork.Common.Util;

namespace Domain.SeedWork;

public abstract class RelationalDbBaseModel : RelationalDbAuditableModel
{
    public long Id { get; set; }
    public Guid ExternalId { get; set; } = Guid.NewGuid();
    public bool Active { get; set; } = true;

    public Notification ValidateExternalId(string externalId, Notification? notification = null)
    {
        notification ??= new Notification();
        var (externalIdGuid, externalIdNotification) = ParseExternalId(externalId, notification);
        ExternalId = externalIdGuid;
        return externalIdNotification;
    }

    public static (Guid ExternalId, Notification Notification) ParseExternalId(string externalId, Notification? notification = null)
    {
        notification ??= new Notification();

        if (string.IsNullOrWhiteSpace(externalId))
        {
            notification.AddError("External Id must not be empty.");
        }
        else if (!ValidatorUtil.IsValidGuid(externalId))
        {
            notification.AddError("External Id must be a valid GUID.");
        }

        return notification.HasErrors
            ? (Guid.Empty, notification)
            : (Guid.Parse(externalId), notification);
    }
}