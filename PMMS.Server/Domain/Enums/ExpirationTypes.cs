namespace PMMS.Server.Domain.Enums;

public enum ExpirationTypes
{
    None = 1,
    DateOfCompletion = 2,       // default expiration
    ExtensionOfTime = 3,        // extended the default extension
    PerformanceBond = 4,        // yearly expiration (every 12 months)
    SemestralReport = 5         // semestral expiration (every 6 months)
}   
