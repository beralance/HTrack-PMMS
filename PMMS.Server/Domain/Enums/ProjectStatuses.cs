namespace PMMS.Server.Domain.Enums;

public enum ProjectStatuses
{
    None = 1,
    OnGoing = 2,            // project is running with an expiration of DOC
    Extended  = 3,          // project is running with an expiration of EOT
    Completed  = 4,         // project is completed: COC is given
    FullyCompleted = 5,     // project is completed: DOD is given
    Cancelled = 6,          // Project is cancelled within ongoing and extended status
}