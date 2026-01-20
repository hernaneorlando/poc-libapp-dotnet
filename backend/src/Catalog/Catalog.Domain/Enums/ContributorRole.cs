namespace Catalog.Domain.Enums;

/// <summary>
/// Enumeration for contributor roles in a book.
/// </summary>
public enum ContributorRole
{
    /// <summary>Book author</summary>
    Author = 1,
    
    /// <summary>Book illustrator</summary>
    Illustrator = 2,
    
    /// <summary>Book translator</summary>
    Translator = 3,
    
    /// <summary>Book editor</summary>
    Editor = 4,
    
    /// <summary>Book co-author</summary>
    CoAuthor = 5,

    Foreword = 6
}
