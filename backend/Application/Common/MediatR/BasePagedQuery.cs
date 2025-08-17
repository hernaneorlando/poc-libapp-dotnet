using Application.Common.BaseDTO;
using Domain.Common;
using MediatR;

namespace Application.Common.MediatR;

public abstract record BasePagedQuery<TDto> : IRequest<ValidationResult<PagedResponseDTO<TDto>>>
    where TDto : class
{
    public int PageNumber { get; set; }
    public int PageSize { get; set; }
    public string OrderBy { get; set; }
    public bool IsDescending { get; set; }
}