using PMMS.Server.Domain.Enums;

namespace PMMS.Server.Domain.Entities;

public class Expiration
{
    public int Id {get; set;}
    public ExpirationTypes? Type {get; set;}
    public ExpirationStatuses? Status {get; set;}  
    public DateOnly? ExpiresOn {get; set;}
    public DateOnly? LastExpirationDate {get; set;}
    public bool IsActive {get; set;}

    public DateOnly? CompletedAt {get; set;}
    public DateOnly? HandledAt {get; set;}
    public DateTimeOffset? ExpiredAt {get; set;}
    public DateOnly? CancelledAt {get; set;}
    

    public DateTimeOffset CreatedAt {get; set;}
    public DateTimeOffset? UpdatedAt {get; set;}

    public Guid? ProjectId {get; set;}
    public string? HandledById {get; set;}
    public string? CancelledById {get; set;}

    public Project? Project {get; set;}
    public ApplicationUser? HandledBy {get; set;}
    public ApplicationUser? CancelledBy {get; set;}
}