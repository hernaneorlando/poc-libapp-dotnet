using FluentResults;
using MediatR;

namespace Application.SeedWork.MediatR;

public abstract record BaseGetByIdQuery<TDto> : IRequest<Result<TDto>>
    where TDto : class
{
    public string Id { get; set; } = string.Empty;
}
