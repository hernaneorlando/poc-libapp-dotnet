using System;
using Core.API;
using FluentValidation;

namespace Core.Application;

public class BasePagedQueryValidator<TQuery, TResponse> : AbstractValidator<TQuery>
    where TQuery : BasePagedQuery<TResponse>
    where TResponse : class
{
    public BasePagedQueryValidator()
    {
        RuleFor(x => x.PageNumber)
            .GreaterThanOrEqualTo(1)
            .WithMessage("Page number must be greater than or equal to 1.");

        RuleFor(x => x.PageSize)
            .LessThanOrEqualTo(100)
            .WithMessage("Page size must be less than or equal to 100.");
    }
}
