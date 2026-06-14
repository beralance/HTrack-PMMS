namespace PMMS.Server.Domain.Enums;

public enum ProjectSeverities
{
    None = 1,
    Normal = 2,     // No expired expiration
    Warning = 3,    // 1 expiration type expired
    Critical = 4,   // 2 Expiration type expired
    Severe = 5      // 3 or more expiration type expired
}
