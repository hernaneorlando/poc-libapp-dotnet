using Domain.Common;
using MediatR;

namespace Application.Common.MediatR;

public abstract record BasePagedQuery<TDto> : IRequest<ValidationResult<IEnumerable<TDto>>>
    where TDto : class
{
    public int PageNumber { get; set; }
    public int PageSize { get; set; }
}