using System.Text.Json.Serialization;
using Application.Common.BaseDTO;
using Domain.Common;
using MediatR;

namespace Application.Common;

public abstract record UpdateEntityBaseCommand<TDto> : IRequest<ValidationResult<TDto>>
    where TDto : BaseDto
{
    [JsonIgnore]
    public string Id { get; set; }
}