using FluentResults;
using MediatR;

namespace Application.Common.MediatR;

public abstract record BaseGetByIdQuery<TDto> : IRequest<Result<TDto>>
    where TDto : class
{
    public string Id { get; set; } = string.Empty;
}
