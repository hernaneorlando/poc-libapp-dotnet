using Application.Common;
using Application.Common.BaseDTO;
using Application.Common.MediatR;
using Domain.Common;
using LibraryApp.API.Extensions;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace LibraryApp.API.Endpoints.CatalogManagement;

public class CatalogManagementBaseEndpoints
{
    public static RouteGroupBuilder GetGroupBuilder(WebApplication app, string routeSuffix)
    {
        return app.MapGroup($"api/{routeSuffix}")
            .WithGroupName("Catalog Management");
    }

    public static void AddCreateEntityEndpoint<TCommand, TDto>(RouteGroupBuilder groupBuilder, string entityName)
        where TCommand : IRequest<ValidationResult<TDto>>
        where TDto : BaseDto
    {
        groupBuilder.MapPost("", async (
            [FromServices] IMediator mediator,
            [FromBody] TCommand command
        ) =>
        {
            var result = await mediator.Send(command);
            return result.Match(
                category => Results.CreatedAtRoute($"Get{entityName}ById", new { id = category.Id }, category),
                errors =>
                {
                    return Results.BadRequest(new ResultError(
                        Title: $"{entityName} creation failed",
                        Details: string.Join($",{Environment.NewLine}", errors),
                        StatusCode: StatusCodes.Status400BadRequest
                    ));
                }
            );
        })
        .Produces<TDto>(StatusCodes.Status200OK)
        .Produces<ResultError>(StatusCodes.Status400BadRequest)
        .WithName($"Create{entityName}");
    }

    public static void AddGetEntityByIdEndpoint<TQuery, TDto>(RouteGroupBuilder groupBuilder, string entityName)
        where TQuery : BaseGetByIdQuery<TDto>
        where TDto : BaseDto
    {
        groupBuilder.MapGet("{id}", async (
            [FromServices] IMediator mediator,
            string id
        ) =>
        {
            var query = Activator.CreateInstance<TQuery>() as BaseGetByIdQuery<TDto>;
            query.Id = id;

            var result = await mediator.Send(query);
            return result.Match(
                Results.Ok,
                errors =>
                {
                    return Results.NotFound(new ResultError(
                        Title: $"{entityName} not found",
                        Details: string.Join($",{Environment.NewLine}", errors),
                        StatusCode: StatusCodes.Status404NotFound
                    ));
                }
            );
        })
        .Produces<TDto>(StatusCodes.Status200OK)
        .Produces<ResultError>(StatusCodes.Status404NotFound)
        .WithName($"Get{entityName}ById");
    }

    public static void AddGetPagedEntitiesEndpoint<TQuery, TDto>(RouteGroupBuilder groupBuilder, string entityPluralName)
        where TQuery : BasePagedQuery<TDto>
        where TDto : BaseDto
    {
        groupBuilder.MapPost("query", async (
            [FromServices] IMediator mediator,
            [FromBody] TQuery query
        ) =>
        {
            var result = await mediator.Send(query);
            return result.Match(
                Results.Ok,
                errors =>
                {
                    return Results.NotFound(new ResultError(
                        Title: $"{entityPluralName} not found",
                        Details: string.Join($",{Environment.NewLine}", errors),
                        StatusCode: StatusCodes.Status404NotFound
                    ));
                }
            );
        })
        .Produces<TDto>(StatusCodes.Status200OK)
        .Produces<ResultError>(StatusCodes.Status404NotFound)
        .WithName($"Get{entityPluralName}");
    }

    public static void AddUpdateEntityEndpoint<TCommand, TDto>(RouteGroupBuilder groupBuilder, string entityName)
        where TCommand : UpdateEntityBaseCommand<TDto>
        where TDto : BaseDto
    {
        groupBuilder.MapPut("{id}", async (
            [FromServices] IMediator mediator,
            string id,
            [FromBody] TCommand command
        ) =>
        {
            command.Id = id;
            var result = await mediator.Send(command);
            return result.Match(
                Results.Ok,
                errors =>
                {
                    return Results.BadRequest(new ResultError(
                        Title: $"{entityName} update failed",
                        Details: string.Join($",{Environment.NewLine}", errors),
                        StatusCode: StatusCodes.Status400BadRequest
                    ));
                }
            );
        })
        .Produces<TDto>(StatusCodes.Status200OK)
        .Produces<ResultError>(StatusCodes.Status400BadRequest)
        .WithName($"Update{entityName}");
    }

    public static void AddDeleteEntityEndpoint<TCommand, TDto>(RouteGroupBuilder groupBuilder, string entityName)
        where TCommand : DeleteEntityBaseCommand
        where TDto : BaseDto
    {
        groupBuilder.MapDelete("{id}", async (
            [FromServices] IMediator mediator,
            string id
        ) =>
        {
            var command = Activator.CreateInstance(typeof(TCommand), id) as DeleteEntityBaseCommand;
            var result = await mediator.Send(command);

            return result.Match(
                Results.NoContent,
                errors =>
                {
                    return Results.BadRequest(new ResultError(
                        Title: $"{entityName} deletion failed",
                        Details: string.Join($",{Environment.NewLine}", errors),
                        StatusCode: StatusCodes.Status400BadRequest
                    ));
                }
            );
        })
        .Produces<TDto>(StatusCodes.Status200OK)
        .Produces<ResultError>(StatusCodes.Status400BadRequest)
        .WithName($"{entityName}Category");
    }
}