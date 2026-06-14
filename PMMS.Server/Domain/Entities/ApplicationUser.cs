using Microsoft.AspNetCore.Identity;

namespace PMMS.Server.Domain.Entities;

public class ApplicationUser : IdentityUser
{
    public bool IsTemporary {get; set;}
    public bool? IsDeleted {get; set;}
    public string? DeletedById {get; set;}

    public DateTimeOffset CreatedAt {get; set;}
    public DateTimeOffset? UpdatedAt {get; set;}
    public DateTimeOffset? DeletedAt {get; set;}
}