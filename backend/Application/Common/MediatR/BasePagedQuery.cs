using FluentResults;
using MediatR;

namespace Application.Common.MediatR;

public abstract record BasePagedQuery<TDto> : IRequest<Result<IEnumerable<TDto>>>
    where TDto : class
{
    public int PageNumber { get; set; }
    public int PageSize { get; set; }
}