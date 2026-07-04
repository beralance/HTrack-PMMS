using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;
using PMMS.Server.Common.Endpoints;
using PMMS.Server.Common.Result;
using PMMS.Server.Common.Security;
using PMMS.Server.Infrastructure.Persistence;

namespace PMMS.Server.Features.Drafts;


public sealed class DeleteDraft : IEndpoint {

    public record Command(Guid Id)
        : IRequest<AppResult<Response>>;
    
    public record Response(
        Guid Id,
        string Message);


    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapDelete("drafts/{id:guid}", async (Guid id, ISender sender, CancellationToken ct) =>
            {
                var result = await sender.Send(new Command(id), ct);
                
                return result.IsSuccess 
                    ? TypedResults.Ok(result)
                    : result.ToProblem();
            })
            .WithName("DeleteDraft")
            .WithTags("Drafts")
            .WithSummary("Delete draft")
            .RequireAuthorization("TemporaryOnly")
            .Produces(StatusCodes.Status200OK)
            .ProducesProblem(StatusCodes.Status404NotFound);
    }


    public class Handler(PmmsDbContext context, IUserContext userContext)
    : IRequestHandler<Command, AppResult<Response>>
    {
        public async Task<AppResult<Response>> Handle(Command command, CancellationToken ct)
        {
            var userId = userContext.UserId;

            var draft = await context.Drafts
                .Include(d => d.Municipality)
                .FirstOrDefaultAsync(d => d.Id == command.Id && d.IsPublished == false, ct);

            if (draft is null)
            {
                return AppResult<Response>.Failure($"Draft {command.Id} was not found or has already been published.", ErrorType.NotFound);
            }

            bool isOwner = draft.AddedById == userId;

            if (!isOwner)
            {
                return AppResult<Response>.Failure("You do not have permission to delete this draft.", ErrorType.Forbidden);
            }
                
            context.Drafts.Remove(draft);
            await context.SaveChangesAsync(ct);

            var res = new Response(
                Id: draft.Id,
                Message: "Draft deleted successfully." 
            );

            return AppResult<Response>.Success(res);
        }
    }
}