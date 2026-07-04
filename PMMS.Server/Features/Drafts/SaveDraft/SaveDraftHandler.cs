using MediatR;
using PMMS.Server.Common.Result;
using PMMS.Server.Common.Security;
using PMMS.Server.Infrastructure.Persistence;
using PMMS.Server.Common.Helper;
using Microsoft.EntityFrameworkCore;
using PMMS.Server.Common.Behaviors;

namespace PMMS.Server.Features.Drafts.SaveDraft;

public sealed class SaveDraftHandler(
    PmmsDbContext context, 
    IUserContext userContext, 
    IMediator mediator
) : IRequestHandler<SaveDraftCommand, AppResult<SaveDraftResponse>>
{
    public async Task<AppResult<SaveDraftResponse>> Handle(SaveDraftCommand command, CancellationToken ct)
    {
        var userId = userContext.UserId;
        if (userId == null) 
            return AppResult<SaveDraftResponse>.Failure("Unauthorized.", ErrorType.Unauthorized);

        var draft = await context.Drafts
            .Include(d => d.Municipality)
            .FirstOrDefaultAsync(d => d.Id == command.Id && d.AddedById == userId && d.IsPublished == false, ct);
        
        if (draft == null) 
            return AppResult<SaveDraftResponse>.Failure("Draft not found.", ErrorType.NotFound);

        await using var transaction = await context.Database.BeginTransactionAsync(ct);
        try
        {
            var now = DateTimeOffset.UtcNow;
            var status = SaveDraftBehavior.DetermineProjectStatus(draft);

            Console.WriteLine($"Status: {status}");
            var project = SaveDraftBehavior.MapProject(draft, status, userId, now);
            context.Projects.Add(project);

            var expirations = CreateExpirationBehavior.CalculateExpirations(
                project,
                status,
                draft.DateOfCompletion,
                draft.ExtensionOfTime
            );

            context.Expirations.AddRange(expirations);

            draft.IsPublished = true;
            draft.PublishedAt = now;
            draft.PublishedById = userId;

            await context.SaveChangesAsync(ct);
            await transaction.CommitAsync(ct);

            await mediator.Publish(new DraftCreatedEvent(project.Id, project.Municipality.ProvinceId, userId), ct);

            return AppResult<SaveDraftResponse>.Success(new SaveDraftResponse(
                project.Id, "Draft published successfully to Projects collection."));
        }
        catch (Exception)
        {
            await transaction.RollbackAsync(ct);
            throw;
        }
    }
}