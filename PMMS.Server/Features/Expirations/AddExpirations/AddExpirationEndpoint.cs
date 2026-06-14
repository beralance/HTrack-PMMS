using Mapster;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using PMMS.Server.Common.Endpoints;
using PMMS.Server.Common.Result;

namespace PMMS.Server.Features.Expirations.AddExpirations;

public class AddExpirationEndpoint : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapPost("expirations/{projectId:guid}", async(
            [FromRoute] Guid projectId, 
            [FromBody] AddExpirationRequest request, 
            ISender sender, 
            CancellationToken ct) =>
        {
            var command = new AddExpirationCommand(
                ProjectId: projectId,
                CocDate: request.CocDate,
                DodDate: request.DodDate,
                HasCoc: request.CocDate.HasValue,
                HasDod: request.DodDate.HasValue,
                DateOfCompletion: request.DateOfCompletion,
                ExtensionOfTime: request.ExtensionOfTime
            );

            var addExpirationCommand = command with {ProjectId = projectId};

            var result = await sender.Send(addExpirationCommand, ct);
                
            return result.IsSuccess
                ? TypedResults.Created($"/expirations/{result.Value?.Id}", result)
                : result.ToProblem();
        })
        .WithName("AddExpirations")
        .WithTags("Expirations")
        .WithSummary("Add expirations")
        .RequireAuthorization("PermanentOnly")
        .Produces(StatusCodes.Status201Created)
        .ProducesValidationProblem()
        .ProducesProblem(StatusCodes.Status403Forbidden)
        .ProducesProblem(StatusCodes.Status404NotFound)
        .ProducesProblem(StatusCodes.Status409Conflict)
        .ProducesProblem(StatusCodes.Status401Unauthorized)
        .ProducesProblem(StatusCodes.Status422UnprocessableEntity);
    }
}