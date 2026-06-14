using MediatR;
using Microsoft.AspNetCore.Mvc;
using PMMS.Server.Common.Endpoints;
using PMMS.Server.Common.Result;

namespace PMMS.Server.Features.Expirations.ResetExpiration;

public sealed class UpdateExpirations : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapPut("expirations/{id:guid}", async (Guid id, [FromBody] ResetExpirationCommand command, ISender sender, CancellationToken ct) =>
        {
            var updateExpirationCommand = command with { Id = id };

            var result = await sender.Send(updateExpirationCommand, ct);
                
            return result.IsSuccess
                ? TypedResults.Ok(result)
                : result.ToProblem();
        })
        .WithName("ResetExpiration")
        .WithTags("Expirations")
        .WithSummary("Reset expiration")
        .RequireAuthorization("PermanentOnly")
        .Produces(StatusCodes.Status200OK)
        .ProducesValidationProblem()
        .ProducesProblem(StatusCodes.Status403Forbidden)
        .ProducesProblem(StatusCodes.Status404NotFound)
        .ProducesProblem(StatusCodes.Status401Unauthorized)
        .ProducesProblem(StatusCodes.Status422UnprocessableEntity);
    }
}