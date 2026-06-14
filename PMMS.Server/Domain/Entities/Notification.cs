using PMMS.Server.Domain.Enums;

namespace PMMS.Server.Domain.Entities;

public class Notification
{
    public int Id {get; set;}
    public string Title {get; set;} = string.Empty;
    public string Content {get; set;} = string.Empty;
    public NotificationTypes? Type {get; set;}
    public bool? IsRead {get; set;}
    public bool? IsDeleted {get; set;}

    public DateTimeOffset? DeletedAt {get; set;}
    public DateTimeOffset? ReadAt {get; set;}
    public DateTimeOffset CreatedAt {get; set;}

    public string? UserId {get; set;}
    public string? DeletedById {get; set;}
    public Guid? ProjectId {get; set;}
    public int ProvinceId {get; set;}
    public string? SenderId {get; set;}


    public ApplicationUser? User {get; set;}
    public ApplicationUser? DeletedBy {get; set;}
    public Project? Project {get; set;}
    public Province Province {get; set;} = null!;
    public ApplicationUser? Sender {get; set;}
}
