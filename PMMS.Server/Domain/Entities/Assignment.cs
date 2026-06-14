namespace PMMS.Server.Domain.Entities;

public class Assignment
{
    public int Id {get; set;}

    public DateTimeOffset CreatedAt {get; set;}
    public DateTimeOffset? UpdatedAt {get; set;}

    public string? UserId {get; set;} = string.Empty;
    public int ProvinceId {get; set;}

    public ApplicationUser? User {get; set;}
    public Province Province {get; set;} = null!;
}

