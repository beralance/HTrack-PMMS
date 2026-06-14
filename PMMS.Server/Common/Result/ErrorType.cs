namespace PMMS.Server.Common.Result;

public enum ErrorType
{
    None = 0,
    Failure = 1,
    Validation = 2,
    NotFound = 3, 
    Conflict = 4, 
    Forbidden = 5,  
    Unauthorized = 6
}