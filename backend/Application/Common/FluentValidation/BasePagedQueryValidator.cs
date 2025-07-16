using Application.Common.MediatR;
using FluentValidation;

namespace Application.Common.FluentValidation;

public abstract class BasePagedQueryValidator<TQuery, TDto> : AbstractValidator<TQuery>
    where TQuery : BasePagedQuery<TDto>
    where TDto : class
{
    public BasePagedQueryValidator()
    {
        RuleFor(x => x.PageNumber)
            .GreaterThan(0)
            .WithMessage("Page number must be greater than 0.");

        RuleFor(x => x.PageSize)
            .InclusiveBetween(1, 100)
            .WithMessage("Page size must be between 1 and 100.");
    }
}