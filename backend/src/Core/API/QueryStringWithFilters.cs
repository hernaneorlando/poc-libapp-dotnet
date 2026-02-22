using System.Diagnostics.CodeAnalysis;
using Microsoft.AspNetCore.Http;

namespace Core.API;

public class QueryStringWithFilters<TQuery, TResponse> : Dictionary<string, List<string>>, IParsable<QueryStringWithFilters<TQuery, TResponse>>
    where TQuery : BasePagedQuery<TResponse>, new()
    where TResponse : class
{
    private const string PageNumberKey = "_page";
    private const string PageSizeKey = "_size";
    private const string OrderByKey = "_order";

    /// <summary>
    /// Enables automatic binding from HttpContext in ASP.NET Core minimal APIs.
    /// This method is automatically discovered and called by the minimal API framework.
    /// </summary>
    public static ValueTask<QueryStringWithFilters<TQuery, TResponse>> BindAsync(HttpContext context)
    {
        var queryString = context.Request.QueryString.Value ?? string.Empty;
        TryParse(queryString, null, out var result);
        return ValueTask.FromResult(result ?? []);
    }

    public BasePagedQuery<TResponse> GetQuery()
    {
        var query = new TQuery();

        if (ContainsKey(PageNumberKey) || ContainsKey(nameof(BasePagedQuery<>.PageNumber)))
        {
            var value = SanitizeValues(this[PageNumberKey].FirstOrDefault()!);
            query.PageNumber = int.TryParse(value, out int page) ? page : 0;
            Remove(PageNumberKey);
        }

        if (ContainsKey(PageSizeKey) || ContainsKey(nameof(BasePagedQuery<>.PageSize)))
        {
            var value = SanitizeValues(this[PageSizeKey].FirstOrDefault()!);
            query.PageSize = int.TryParse(value, out int page) ? page : 0;
            Remove(PageSizeKey);
        }

        if (ContainsKey(OrderByKey) || ContainsKey(nameof(BasePagedQuery<>.OrderBy)))
        {
            query.OrderBy = SanitizeValues(this[OrderByKey].FirstOrDefault()!);
            Remove(OrderByKey);
        }

        foreach (var key in Keys)
        {
            var values = this[key];
            if (values.Count > 1)
                query.AddFilter(key, values.Select(SanitizeValues));
            else
                query.AddFilter(key, SanitizeValues(values.First()));
        }

        return query;
    }

    private static string SanitizeValues(string value)
    {
        // Just for swagger context
        return value
            .Replace("[\"", string.Empty)
            .Replace("\"]", string.Empty);
    }

    /// <summary>
    /// Parses a query string into a QueryStringWithFilters instance.
    /// </summary>
    /// <param name="s">Query string (e.g., "page=1&size=10&order=Name&search=test")</param>
    /// <param name="provider">Format provider (unused)</param>
    /// <returns>Parsed QueryStringWithFilters instance</returns>
    /// <exception cref="FormatException">Thrown when the query string format is invalid</exception>
    public static QueryStringWithFilters<TQuery, TResponse> Parse(string s, IFormatProvider? provider)
    {
        if (!TryParse(s, provider, out var result))
        {
            throw new FormatException($"Failed to parse query string: '{s}'");
        }

        return result!;
    }

    /// <summary>
    /// Attempts to parse a query string into a QueryStringWithFilters instance.
    /// </summary>
    /// <param name="s">Query string (e.g., "page=1&size=10&order=Name&search=test")</param>
    /// <param name="provider">Format provider (unused)</param>
    /// <param name="result">Parsed QueryStringWithFilters instance if successful; otherwise null</param>
    /// <returns>True if parsing succeeded; otherwise false</returns>
    public static bool TryParse([NotNullWhen(true)] string? s, IFormatProvider? provider, [MaybeNullWhen(false)] out QueryStringWithFilters<TQuery, TResponse> result)
    {
        result = null;

        // Handle empty or null strings
        if (string.IsNullOrWhiteSpace(s))
        {
            result = [];
            return true;
        }

        try
        {
            var queryString = new QueryStringWithFilters<TQuery, TResponse>();

            // Remove leading '?' if present
            var queryPart = s.StartsWith('?') ? s[1..] : s;

            if (string.IsNullOrWhiteSpace(queryPart))
            {
                result = queryString;
                return true;
            }

            // Split by '&' to get individual parameters
            var parameters = queryPart.Split('&', StringSplitOptions.RemoveEmptyEntries);

            foreach (var parameter in parameters)
            {
                var parts = parameter.Split('=', 2, StringSplitOptions.RemoveEmptyEntries);

                if (parts.Length == 0)
                    continue;

                var key = Uri.UnescapeDataString(parts[0]);
                var value = parts.Length > 1 ? Uri.UnescapeDataString(parts[1]) : string.Empty;

                // Add or append value to the dictionary
                if (queryString.TryGetValue(key, out List<string>? value1))
                {
                    value1.Add(value);
                }
                else
                {
                    queryString[key] = [value];
                }
            }

            result = queryString;
            return true;
        }
        catch
        {
            result = null;
            return false;
        }
    }
}
