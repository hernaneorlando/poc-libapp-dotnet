using Domain.Common;
using MediatR;

namespace Application.Common.MediatR;

public abstract record BaseGetByIdQuery<TDto> : IRequest<ValidationResult<TDto>>
    where TDto : class
{
    public string Id { get; set; } = string.Empty;
}
