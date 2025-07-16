using Domain.Common.Util;

namespace Domain.Common;

public abstract class RelationalDbBaseModel<TModel> : RelationalDbAuditableModel
    where TModel : RelationalDbBaseModel<TModel>
{
    public long Id { get; set; }
    public Guid ExternalId { get; set; } = Guid.NewGuid();
    public bool Active { get; set; } = true;

    public static ValidationResult<Guid> ParseExternalId(string externalId, ValidationResult<TModel>? result = null)
    {
        result ??= ValidationResult.Create<TModel>();

        if (string.IsNullOrWhiteSpace(externalId))
            result.AddError("External Id must not be empty.");
        else if (!ValidatorUtil.IsValidGuid(externalId))
            result.AddError("External Id must be a valid GUID.");

        if (result.IsSuccess)
        {
            var guidResult = ValidationResult.Create<Guid>();
            guidResult.AddValue(Guid.Parse(externalId));
            return guidResult;
        }

        return ValidationResult.Fail<Guid>(result.Errors);
    }

    public ValidationResult<TModel> Deactivate(string externalId, ValidationResult<TModel>? result = null)
    {
        result ??= ValidationResult.Create<TModel>();

        ValidateExternalId(externalId, result);
        Active = false;
        UpdatedAt = DateTime.UtcNow;

        if (!result.IsSuccess)
            return result;

        result.AddValue((TModel)this);
        return result;
    }

    public ValidationResult<TModel> Deactivate(TModel model)
    {
        var result = ValidationResult.Create<TModel>();
        if (model is null)
        {
            result.AddError($"{typeof(TModel).Name} cannot be null.");
            return result;
        }

        if (model.ExternalId != ExternalId)
        {
            result.AddError($"{typeof(TModel).Name} must have the same External Id.");
            return result;
        }

        if (!Active)
        {
            result.AddError($"{typeof(TModel).Name} is alredy deactivated.");
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