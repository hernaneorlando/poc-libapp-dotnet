using System;
using Domain.Common.Interfaces;
using Domain.Common.Util;

namespace Domain.Common;

public abstract class DbBaseModel<TModel> : IAuditableModel
    where TModel : DbBaseModel<TModel>
{
    public Guid ExternalId { get; set; } = Guid.NewGuid();
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? UpdatedAt { get; set; }
    public bool Active { get; set; } = true;

    public static ValidationResult<Guid> ParseExternalId(string externalId, ValidationResult<TModel>? result = null)
    {
        result ??= ValidationResult.Create<TModel>();

        if (string.IsNullOrWhiteSpace(externalId))
            result.AddError("External Id must not be empty");
        else if (!ValidatorUtil.IsValidGuid(externalId))
            result.AddError("External Id must be a valid GUID");

        var guidResult = ValidationResult.Create<Guid>(result.Errors);
        if (Guid.TryParse(externalId, out var guid))
            guidResult.AddValue(guid);

        return guidResult;
    }

    public ValidationResult<TModel> Deactivate(string externalId, ValidationResult<TModel>? result = null)
    {
        result ??= ValidationResult.Create<TModel>();

        ValidateExternalId(externalId, result);
        Active = false;
        UpdatedAt = DateTime.UtcNow;

        if (result.IsFailure)
            return result;

        result.AddValue((TModel)this);
        return result;
    }
    
    public ValidationResult<TModel> Deactivate(TModel model)
    {
        var result = ValidationResult.Create<TModel>();
        if (model is null)
        {
            result.AddError($"{typeof(TModel).Name} cannot be null");
            return result;
        }

        if (model.ExternalId != ExternalId)
        {
            result.AddError($"{typeof(TModel).Name} must have the same External Id");
            return result;
        }

        if (!Active)
        {
            result.AddError($"{typeof(TModel).Name} is alredy archived");
            return result;
        }

        return Deactivate(model.ExternalId.ToString(), result);
    }

    protected ValidationResult<TModel> ValidateExternalId(string externalId, ValidationResult<TModel>? result = null)
    {
        result ??= ValidationResult.Create<TModel>();
        var guidResult = ParseExternalId(externalId, result);
        ExternalId = guidResult.Value;
        return result;
    }
}
