namespace PMMS.Server.Common.Result;

public static class AppResultExtensions
{
    public static IResult ToProblem(this AppResult result)
    {
        // Guard clause: We should never call ToProblem on a successful result
        if (result.IsSuccess)
        {
            throw new InvalidOperationException("Cannot convert a successful result to a problem.");
        }

        return result.ErrorType switch
        {
            // 422 Unprocessable Entity - Ideal for FluentValidation
            ErrorType.Validation => Results.ValidationProblem(
                errors: result.ValidationErrors,
                detail: result.Error,
                instance: null,
                title: "Validation Error"),

            // 404 Not Found - User or Province missing in Bicol DB
            ErrorType.NotFound => Results.Problem(
                detail: result.Error,
                statusCode: StatusCodes.Status404NotFound,
                title: "Resource Not Found"),

            // 409 Conflict - e.g., Assignment already exists
            ErrorType.Conflict => Results.Problem(
                detail: result.Error,
                statusCode: StatusCodes.Status409Conflict,
                title: "Logic Conflict"),

            // 403 Forbidden - OJT/Intern trying to access Admin features
            ErrorType.Forbidden => Results.Problem(
                detail: result.Error,
                statusCode: StatusCodes.Status403Forbidden,
                title: "Access Denied"),
                
            // 401 Unauthorized
            ErrorType.Unauthorized => Results.Problem(
                detail: result.Error,
                statusCode: StatusCodes.Status401Unauthorized,
                title: "Not Authorized"),

            // 400 Bad Request - Generic business rule failure
            _ => Results.BadRequest(new { result.Error, result.IsSuccess })
        };
    }
}