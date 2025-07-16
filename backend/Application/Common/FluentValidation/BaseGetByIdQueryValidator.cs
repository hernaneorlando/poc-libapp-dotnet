using Application.Common.MediatR;
using Domain.Common.Util;
using FluentValidation;

namespace Application.Common.FluentValidation;

public abstract class BaseGetByIdQueryValidator<TQuery, TDto> : AbstractValidator<TQuery>
    where TQuery : BaseGetByIdQuery<TDto>
    where TDto : class
{
    public BaseGetByIdQueryValidator()
    {
        RuleFor(x => x.Id)
            .NotEmpty().WithMessage("Id must not be empty.")
            .Must(ValidatorUtil.IsValidGuid).WithMessage("Id must be a valid GUID.");
    }
}