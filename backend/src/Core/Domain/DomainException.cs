namespace Core.Domain;

/// <summary>
/// Exception that should be thrown when a domain invariant is violated.
/// Unlike FluentResults which handles expected errors, DomainException is for truly unexpected situations.
/// 
/// Examples:
/// - Attempt to access a required property that is null (invariant violated)
/// - Business algorithm that fell into an impossible state
/// 
/// Expected errors (user not found, insufficient permission, etc):
///   → Use FluentResults.Result.Fail() instead of throwing an exception
/// </summary>
public class DomainException : Exception
{
    /// <summary>
    /// Name or type of the aggregate where the invariant was violated.
    /// </summary>
    public string? AggregateType { get; set; }

    /// <summary>
    /// Technical details of the invariant that was violated.
    /// </summary>
    public string? InvariantDescription { get; set; }

    public DomainException(string message) : base(message)
    {
    }

    public DomainException(string message, Exception innerException) 
        : base(message, innerException)
    {
    }

    public DomainException(string message, string aggregateType, string invariantDescription)
        : base(message)
    {
        AggregateType = aggregateType;
        InvariantDescription = invariantDescription;
    }

    public DomainException(string message, string aggregateType, string invariantDescription, Exception innerException)
        : base(message, innerException)
    {
        AggregateType = aggregateType;
        InvariantDescription = invariantDescription;
    }

    public override string ToString()
    {
        var result = base.ToString();
        
        if (!string.IsNullOrEmpty(AggregateType))
            result += $"\nAggregate Type: {AggregateType}";
        
        if (!string.IsNullOrEmpty(InvariantDescription))
            result += $"\nInvariant Violated: {InvariantDescription}";

        return result;
    }
}
