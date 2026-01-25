using System.Text.Json.Serialization;
using Core.Infrastructure;
using Core.Infrastructure.Enums;
using MediatR;

namespace Core.API;

public record BasePagedQuery<TResponse> : IRequest<PaginatedResponse<TResponse>>
    where TResponse : class
{
    [JsonPropertyName("_page")]
    public int PageNumber { get; set; } = 1;

    [JsonPropertyName("_size")]
    public int PageSize { get; set; } = 10;

    [JsonPropertyName("_order")]
    public string OrderBy { get; set; } = string.Empty;

    [JsonIgnore]
    public List<FilterCriteria> Filters { get; set; } = [];

    public void AddFilter(string field, string value)
    {
        if (field.StartsWith("_min"))
            Filters.Add(new FilterCriteria(field[4..], value, FilterOperator.GreaterThanOrEqual));
        else if (field.StartsWith("_max"))
            Filters.Add(new FilterCriteria(field[4..], value, FilterOperator.LessThanOrEqual));
        else
            Filters.Add(FilterCriteria.Parse(field, value));
    }

    public void AddFilter(string field, IEnumerable<string> values)
    {
        Filters.Add(FilterCriteria.Parse(field, values));
    }
}
