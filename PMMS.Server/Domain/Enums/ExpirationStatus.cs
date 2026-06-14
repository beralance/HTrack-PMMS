namespace PMMS.Server.Domain.Enums;

public enum ExpirationStatuses
{
    None = 1,
    Ongoing = 2,            // expiration is running
    Completed = 3,          // expiraiton is completed
    Extended = 4,           // expiration is extended
    NearExpiration = 5,     // expiration is near expiration
    Expired = 6,            // expiraiton is expired
    Cancelled = 7           // expiration is cancelled
}
