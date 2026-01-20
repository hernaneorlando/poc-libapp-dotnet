namespace Catalog.Domain.Enums;

/// <summary>
/// Enumeration for book status in the catalog.
/// </summary>
public enum BookStatus
{
    /// <summary>Book is available for checkout</summary>
    Available = 1,
    
    /// <summary>Book is not available (all copies checked out or damaged)</summary>
    Unavailable = 2,
    
    /// <summary>Book is reserved by a user</summary>
    Reserved = 3,
    
    /// <summary>Book was lost</summary>
    Lost = 4,
    
    /// <summary>Book is damaged and not available</summary>
    Damaged = 5,
    
    /// <summary>Book is inactive and removed from catalog</summary>
    Inactive = 6
}