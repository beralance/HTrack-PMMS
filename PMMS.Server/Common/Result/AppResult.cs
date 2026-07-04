namespace PMMS.Server.Common.Result;

public class AppResult
{
    public bool IsSuccess { get; }
    public bool IsFailure => !IsSuccess;
    public string Error { get; }
    public ErrorType ErrorType { get; }
    
    public Dictionary<string, string[]> ValidationErrors { get; }

    public AppResult(bool isSuccess, string error, ErrorType errorType, Dictionary<string, string[]>? validationErrors = null)
    {
        IsSuccess = isSuccess;
        Error = error;
        ErrorType = errorType;
        ValidationErrors = validationErrors ?? [];
    }

    public static AppResult Success() => new(true, string.Empty, ErrorType.None);
    
    public static AppResult Failure(string error, ErrorType errorType = ErrorType.Failure) 
        => new(false, error, errorType);

    public static AppResult Invalid(Dictionary<string, string[]> errors, string message = "Validation failed") 
        => new(false, message, ErrorType.Validation, errors);
}

public class AppResult<T> : AppResult
{
    public T? Value { get; }

    public AppResult(T? value, bool isSuccess, string error, ErrorType errorType, Dictionary<string, string[]>? validationErrors = null) 
        : base(isSuccess, error, errorType, validationErrors)
    {
        Value = value;
    }

    public static AppResult<T> Success(T value) => new(value, true, string.Empty, ErrorType.None);
    
    public new static AppResult<T> Failure(string error, ErrorType errorType = ErrorType.Failure) 
        => new(default, false, error, errorType);

    public new static AppResult<T> Invalid(Dictionary<string, string[]> errors, string message = "Validation failed") 
        => new(default, false, message, ErrorType.Validation, errors);
}
