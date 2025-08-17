namespace LibraryApp.Web.Model;

public abstract record AuditableDto
{
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
}
