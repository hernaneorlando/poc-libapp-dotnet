#pragma warning disable CS0693, CS8600, CS8603

using MediatR;
using Microsoft.Extensions.Logging;

namespace Common;

/// <summary>
/// Simple mediator implementation to avoid MediatR licensing issues - For handlers with 2 parameters
/// </summary>
public class SimpleMediator<TRepository, THandler, TRequest, TResponse>(TRepository _roleRepository, ILogger<THandler> _logger) : IMediator
    where TRepository : class
    where THandler : class, IRequestHandler<TRequest, TResponse>
    where TRequest : IRequest<TResponse>
{

    public async Task<TResponse> Send<TResponse>(IRequest<TResponse> request, CancellationToken cancellationToken = default)
    {
        // Handle TRequest specifically
        if (request is TRequest createRoleCommand)
        {
            var handler = Activator.CreateInstance(typeof(THandler), _roleRepository, _logger) as THandler;
            return (TResponse)(object)await handler!.Handle(createRoleCommand, cancellationToken);
        }

        throw new NotSupportedException($"Command type {request.GetType().Name} is not supported");
    }

    public async Task Send<TRequest>(TRequest request, CancellationToken cancellationToken = default) where TRequest : IRequest
    {
        // Handle void request types
        if (request is IRequest unitRequest)
        {
            await Send(unitRequest, cancellationToken);
            return;
        }
        throw new NotSupportedException($"Request type {typeof(TRequest).Name} not supported");
    }

    public Task Publish<TNotification>(TNotification notification, CancellationToken cancellationToken = default) where TNotification : INotification
    {
        return Task.CompletedTask;
    }

    public Task<object?> Send(object request, CancellationToken cancellationToken = default)
    {
        throw new NotImplementedException();
    }

    public IAsyncEnumerable<TResponse> CreateStream<TResponse>(IStreamRequest<TResponse> request, CancellationToken cancellationToken = default)
    {
        throw new NotImplementedException();
    }

    public IAsyncEnumerable<object?> CreateStream(object request, CancellationToken cancellationToken = default)
    {
        throw new NotImplementedException();
    }

    public Task Publish(object notification, CancellationToken cancellationToken = default)
    {
        return Task.CompletedTask;
    }
}

/// <summary>
/// Simple mediator implementation to avoid MediatR licensing issues - For handlers with 3 parameters
/// </summary>
public class SimpleMediator<TRepository, THandler, TRequest, TResponse, TDependency>(TRepository _roleRepository, ILogger<THandler> _logger, TDependency _dependency) : IMediator
    where TRepository : class
    where THandler : class, IRequestHandler<TRequest, TResponse>
    where TRequest : IRequest<TResponse>
    where TDependency : class
{

    public async Task<TResponse> Send<TResponse>(IRequest<TResponse> request, CancellationToken cancellationToken = default)
    {
        // Handle TRequest specifically
        if (request is TRequest createRoleCommand)
        {
            var handler = Activator.CreateInstance(typeof(THandler), _roleRepository, _logger, _dependency) as THandler;
            return (TResponse)(object)await handler!.Handle(createRoleCommand, cancellationToken);
        }

        throw new NotSupportedException($"Command type {request.GetType().Name} is not supported");
    }

    public async Task Send<TRequest>(TRequest request, CancellationToken cancellationToken = default) where TRequest : IRequest
    {
        // Handle void request types
        if (request is IRequest unitRequest)
        {
            await Send(unitRequest, cancellationToken);
            return;
        }
        throw new NotSupportedException($"Request type {typeof(TRequest).Name} not supported");
    }

    public Task Publish<TNotification>(TNotification notification, CancellationToken cancellationToken = default) where TNotification : INotification
    {
        return Task.CompletedTask;
    }

    public Task<object?> Send(object request, CancellationToken cancellationToken = default)
    {
        throw new NotImplementedException();
    }

    public IAsyncEnumerable<TResponse> CreateStream<TResponse>(IStreamRequest<TResponse> request, CancellationToken cancellationToken = default)
    {
        throw new NotImplementedException();
    }

    public IAsyncEnumerable<object?> CreateStream(object request, CancellationToken cancellationToken = default)
    {
        throw new NotImplementedException();
    }

    public Task Publish(object notification, CancellationToken cancellationToken = default)
    {
        return Task.CompletedTask;
    }
}

/// <summary>
/// Simple mediator implementation for handlers that require two dependencies (e.g. UnitOfWork + IMediator)
/// Used by tests to construct handlers with an extra dependency.
/// </summary>
public class SimpleMediator<TRepository, THandler, TRequest, TResponse, TDependency1, TDependency2>(
    TRepository _repository,
    ILogger<THandler> _logger,
    TDependency1 _dep1,
    TDependency2 _dep2) : IMediator
    where TRepository : class
    where THandler : class, IRequestHandler<TRequest, TResponse>
    where TRequest : IRequest<TResponse>
    where TDependency1 : class
    where TDependency2 : class
{
    public async Task<TResponse> Send<TResponse>(IRequest<TResponse> request, CancellationToken cancellationToken = default)
    {
        if (request is TRequest typedRequest)
        {
            var handler = Activator.CreateInstance(typeof(THandler), _repository, _logger, _dep1, _dep2) as THandler;
            return (TResponse)(object)await handler!.Handle(typedRequest, cancellationToken);
        }

        throw new NotSupportedException($"Command type {request.GetType().Name} is not supported");
    }

    public async Task Send<TRequest>(TRequest request, CancellationToken cancellationToken = default) where TRequest : IRequest
    {
        if (request is IRequest unitRequest)
        {
            await Send(unitRequest, cancellationToken);
            return;
        }
        throw new NotSupportedException($"Request type {typeof(TRequest).Name} not supported");
    }

    public Task Publish<TNotification>(TNotification notification, CancellationToken cancellationToken = default) where TNotification : INotification
    {
        return Task.CompletedTask;
    }

    public Task<object?> Send(object request, CancellationToken cancellationToken = default) => throw new NotImplementedException();
    public IAsyncEnumerable<TResponse> CreateStream<TResponse>(IStreamRequest<TResponse> request, CancellationToken cancellationToken = default) => throw new NotImplementedException();
    public IAsyncEnumerable<object?> CreateStream(object request, CancellationToken cancellationToken = default) => throw new NotImplementedException();
    public Task Publish(object notification, CancellationToken cancellationToken = default) => Task.CompletedTask;
}

/// <summary>
/// Simple mediator implementation to avoid MediatR licensing issues - For handlers with 6 parameters
/// </summary>
public class SimpleMediator<TRepository, THandler, TRequest, TResponse, TDependency1, TDependency2, TDependency3, TDependency4>(
    TRepository _repository, 
    ILogger<THandler> _logger, 
    TDependency1 _dep1,
    TDependency2 _dep2,
    TDependency3 _dep3,
    TDependency4 _dep4) : IMediator
    where TRepository : class
    where THandler : class, IRequestHandler<TRequest, TResponse>
    where TRequest : IRequest<TResponse>
    where TDependency1 : class
    where TDependency2 : class
    where TDependency3 : class
    where TDependency4 : class
{

    public async Task<TResponse> Send<TResponse>(IRequest<TResponse> request, CancellationToken cancellationToken = default)
    {
        // Handle TRequest specifically
        if (request is TRequest typedRequest)
        {
            var handler = Activator.CreateInstance(typeof(THandler), _repository, _dep1, _dep2, _dep3, _logger, _dep4) as THandler;
            return (TResponse)(object)await handler!.Handle(typedRequest, cancellationToken);
        }

        throw new NotSupportedException($"Command type {request.GetType().Name} is not supported");
    }

    public async Task Send<TRequest>(TRequest request, CancellationToken cancellationToken = default) where TRequest : IRequest
    {
        // Handle void request types
        if (request is IRequest unitRequest)
        {
            await Send(unitRequest, cancellationToken);
            return;
        }
        throw new NotSupportedException($"Request type {typeof(TRequest).Name} not supported");
    }

    public Task Publish<TNotification>(TNotification notification, CancellationToken cancellationToken = default) where TNotification : INotification
    {
        return Task.CompletedTask;
    }

    public Task<object?> Send(object request, CancellationToken cancellationToken = default)
    {
        throw new NotImplementedException();
    }

    public IAsyncEnumerable<TResponse> CreateStream<TResponse>(IStreamRequest<TResponse> request, CancellationToken cancellationToken = default)
    {
        throw new NotImplementedException();
    }

    public IAsyncEnumerable<object?> CreateStream(object request, CancellationToken cancellationToken = default)
    {
        throw new NotImplementedException();
    }

    public Task Publish(object notification, CancellationToken cancellationToken = default)
    {
        return Task.CompletedTask;
    }
}

#pragma warning restore CS0693, CS8600, CS8603