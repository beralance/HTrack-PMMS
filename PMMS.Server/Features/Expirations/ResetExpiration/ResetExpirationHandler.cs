using MediatR;
using Microsoft.EntityFrameworkCore;
using PMMS.Server.Common.Behaviors;
using PMMS.Server.Common.Helper;
using PMMS.Server.Common.Result;
using PMMS.Server.Common.Security;
using PMMS.Server.Domain.Entities;
using PMMS.Server.Domain.Enums;
using PMMS.Server.Infrastructure.Persistence;

namespace PMMS.Server.Features.Expirations.ResetExpiration;

public class Handler(PmmsDbContext context, IUserContext userContext) : IRequestHandler<ResetExpirationCommand, AppResult<ResetExpirationResponse>>
{
    public async Task<AppResult<ResetExpirationResponse>> Handle(ResetExpirationCommand command, CancellationToken ct)
    {
        // 1. Fetch Project including the Municipality navigation property for scoping checks
        var project = await context.Projects
            .Include(p => p.Municipality)
            .FirstOrDefaultAsync(p => p.Id == command.Id, ct);

        if (project is null)
        {
            return AppResult<ResetExpirationResponse>.Failure($"Project {command.Id} does not exist.", ErrorType.NotFound);
        }

        // 2. Enforce data boundary authorizations based on assigned user provinces
        var userProvinces = userContext.AssignedProvinceIds;
        if (!userProvinces.Contains(project.Municipality.ProvinceId))
        {
            return AppResult<ResetExpirationResponse>.Failure(
                "You do not have permissions to modify projects in this province.", 
                ErrorType.Forbidden
            );
        }

        // 3. Determine business logic status flow safely (handling potential null extensions)
        var projectStatus = command switch
        {
            { HasCoc: true, HasDod: true } => ProjectStatuses.FullyCompleted,
            { HasCoc: true } => ProjectStatuses.Completed,
            { ExtensionOfTime: not null } => ProjectStatuses.Extended,
            _ => ProjectStatuses.OnGoing
        };

        // 4. Update core project properties and synchronize extension flags
        project.CocDate = command.CocDate;
        project.HasCoc = command.HasCoc;
        project.DodDate = command.DodDate;
        project.HasDod = command.HasDod;
        project.Status = projectStatus;
        project.IsExtended = command.ExtensionOfTime.HasValue;
        
        // 5. Log system audit data
        project.IsModified = true;
        project.UpdatedAt = DateTimeOffset.UtcNow;
        project.ModifiedById = userContext.UserId;

        // 6. Purge historical project expirations to allow a clean system calculation state
        var existingExpirations = await context.Expirations
            .Where(e => e.ProjectId == command.Id)
            .ToListAsync(ct);

        if (existingExpirations.Count != 0)
        {
            context.Expirations.RemoveRange(existingExpirations);
        }

        // 7. Recalculate brand-new tracking milestones from the clean source of truth
        var expirations = CreateExpirationBehavior.CalculateExpirations(
            project,
            projectStatus,
            command.DateOfCompletion,
            command.ExtensionOfTime
        );
        
        // 8. Commit updated entities and new tracking rules to database storage
        await context.Expirations.AddRangeAsync(expirations, ct);
        await context.SaveChangesAsync(ct);

        var res = new ResetExpirationResponse(
            Id: project.Id,
            Message: "Project updated and expirations recalculated successfully."
        );

        return AppResult<ResetExpirationResponse>.Success(res);
    }
}