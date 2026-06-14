using MediatR;
using PMMS.Server.Common.Endpoints;
using PMMS.Server.Common.Result;

namespace PMMS.Server.Features.Drafts.SaveDraft;

public sealed class SaveDraft : IEndpoint 
{
    public void MapEndpoint (IEndpointRouteBuilder app)
    {
        app.MapPost("drafts/save", async(SaveDraftCommand command, ISender sender, CancellationToken ct) =>
        {
            var result = await sender.Send(command, ct);   

            return result.IsSuccess
                ? TypedResults.Ok(result)
                : result.ToProblem(); 
        })
        .WithName("SaveDraft")
        .WithTags("Drafts")
        .WithSummary("Save Draft")
        .RequireAuthorization("TemporaryOnly")
        .ProducesValidationProblem()
        .Produces(StatusCodes.Status200OK)
        .ProducesProblem(StatusCodes.Status404NotFound)
        .ProducesProblem(StatusCodes.Status409Conflict);
    }
}