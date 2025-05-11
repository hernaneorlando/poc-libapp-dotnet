using Application.SeedWork.MediatR;
using FluentValidation;

namespace Application.SeedWork.FluentValidation;

public abstract class BaseGetByIdQueryValidator<TQuery, TDto> : AbstractValidator<TQuery>
    where TQuery : BaseGetByIdQuery<TDto>
    where TDto : class
{
    public BaseGetByIdQueryValidator()
    {
        RuleFor(x => x.Id)
            .NotEmpty()
            .WithMessage("Id must not be empty.");
        
        RuleFor(x => x.Id)
            .Must(BeAValidGuid)
            .WithMessage("Id must be a valid GUID.");
    }

    private bool BeAValidGuid(string guid)
    {
        return Guid.TryParse(guid, out _);
    }
}