using System;
using Application.Common.BaseDTO;
using Domain.Common;

namespace Application.Common;

public interface IPagerResponseService<TResponse>
{
    Task<ValidationResult<PagedResponseDTO<TResponse>>> GetActiveEntitiesAsync(int pageNumber, int pageSize, string? orderBy = null, bool? isDescending = null, CancellationToken?  cancellationToken = null);
}
