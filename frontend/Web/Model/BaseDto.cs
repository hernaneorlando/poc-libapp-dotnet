namespace LibraryApp.Web.Model;

public abstract record BaseDto : AuditableDto
{
    public Guid Id { get; set; }
    public bool IsActive { get; set; }
}
