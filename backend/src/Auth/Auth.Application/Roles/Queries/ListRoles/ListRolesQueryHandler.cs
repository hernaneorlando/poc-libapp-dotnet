namespace Auth.Application.Roles.Queries.ListRoles;

using Auth.Application.Roles.DTOs;
using Auth.Infrastructure.Repositories.Interfaces;
using Core.API;

/// <summary>
/// Handler for ListRolesQuery.
/// Retrieves roles with pagination and optional filtering.
/// </summary>
public sealed class ListRolesQueryHandler(
    IRoleRepository _roleRepository,
    ILogger<ListRolesQueryHandler> _logger) : IRequestHandler<ListRolesQuery, PaginatedResponse<RoleDTO>>
{
    public async Task<PaginatedResponse<RoleDTO>> Handle(
        ListRolesQuery request,
        CancellationToken cancellationToken)
    {
        _logger.LogInformation(
            "Listing roles: PageNumber={PageNumber}, PageSize={PageSize}, OrderBy={OrderBy}, OnlyActive={OnlyActive}",
            request.PageNumber,
            request.PageSize,
            request.OrderBy,
            request.ToString());

        // Get all roles (will optimize with pagination in repository later)
        var result = await _roleRepository.GetAllAsync(request, cancellationToken);

        _logger.LogInformation(
            "Retrieved {RoleCount} roles out of {TotalCount} (Page {PageNumber}/{TotalPages})",
            result.Count,
            result.TotalCount,
            request.PageNumber,
            result.TotalPages);

        return new PaginatedResponse<RoleDTO>
        {
            Data = result.Select(r => (RoleDTO)r),
            CurrentPage = request.PageNumber,
            TotalPages = result.TotalPages,
            TotalCount = result.TotalCount,
        };
    }
}
