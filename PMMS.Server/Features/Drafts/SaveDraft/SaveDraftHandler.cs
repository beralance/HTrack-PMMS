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

        // 1. Fetch draft with required navigation
        var draft = await context.Drafts
            .Include(d => d.Municipality)
            .FirstOrDefaultAsync(d => d.Id == command.Id && d.AddedById == userId && d.IsPublished == false, ct);
        
        if (draft == null) 
            return AppResult<SaveDraftResponse>.Failure("Draft not found.", ErrorType.NotFound);

        // 2. Begin Atomic Transaction
        await using var transaction = await context.Database.BeginTransactionAsync(ct);
        try
        {
            var now = DateTimeOffset.UtcNow;
            var status = SaveDraftBehavior.DetermineProjectStatus(draft);
            Console.WriteLine($"Status: {status}");
            // 3. Promote to Project
            var project = SaveDraftBehavior.MapProject(draft, status, userId, now);
            context.Projects.Add(project);

            // 4. Initialize Lifecycle Ledger (Using standard signature)
            var expirations = CreateExpirationBehavior.CalculateExpirations(
                project,
                status,
                draft.DateOfCompletion,
                draft.ExtensionOfTime
            );

            context.Expirations.AddRange(expirations);

            // 5. Finalize Draft State
            draft.IsPublished = true;
            draft.PublishedAt = now;
            draft.PublishedById = userId;

            await context.SaveChangesAsync(ct);
            await transaction.CommitAsync(ct);

            // 6. Notify downstream systems
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