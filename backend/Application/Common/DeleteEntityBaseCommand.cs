using Domain.Common;
using MediatR;

namespace Application.Common;

public abstract record DeleteEntityBaseCommand(string Id) : IRequest<ValidationResult>;