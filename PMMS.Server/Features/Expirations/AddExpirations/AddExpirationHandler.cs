using MediatR;
using Microsoft.EntityFrameworkCore;
using PMMS.Server.Common.Behaviors;
using PMMS.Server.Common.Result;
using PMMS.Server.Infrastructure.Persistence;
using PMMS.Server.Domain.Enums;
using PMMS.Server.Common.Helper;

namespace PMMS.Server.Features.Expirations.AddExpirations;

public class Handler(PmmsDbContext context) : IRequestHandler<AddExpirationCommand, AppResult<AddExpirationResponse>>
{
    public async Task<AppResult<AddExpirationResponse>> Handle(AddExpirationCommand command, CancellationToken ct)
    {
        // 1. Verify project exists
        var project = await context.Projects.FindAsync([command.ProjectId], ct);

        if (project is null)
        {
            return AppResult<AddExpirationResponse>.Failure($"Project {command.ProjectId} does not exist.", ErrorType.NotFound);
        }

        // 2. Prevent re-activation if project already has active expiration trackings
        if (await context.Expirations.AnyAsync(p => p.ProjectId == command.ProjectId, ct) 
            && project.SetupStatus == ProjectSetupStatuses.Active)
        {
            return AppResult<AddExpirationResponse>.Failure(
                $"Project {command.ProjectId} already has an expiration running and is already active.", 
                ErrorType.Conflict
            );
        }


        // 3. Determine initial project status based on incoming completion flags
        var projectStatus = command switch
        {
            { HasCoc: true, HasDod: true } => ProjectStatuses.FullyCompleted,
            { HasCoc: true } => ProjectStatuses.Completed,
            { ExtensionOfTime: not null } => ProjectStatuses.Extended,
            _ => ProjectStatuses.OnGoing
        };

        // 4. Update core project setup state and status parameters
        project.CocDate = command.CocDate;
        project.HasCoc = command.HasCoc;
        project.DodDate = command.DodDate;
        project.HasDod = command.HasDod;
        project.IsExtended = command.ExtensionOfTime.HasValue;
        project.Status = projectStatus;
        project.SetupStatus = ProjectSetupStatuses.Active;

        // 5. Compute the initial set of timeline expirations and milestones
        var expirations = CreateExpirationBehavior.CalculateExpirations(
            project,
            projectStatus,
            command.DateOfCompletion,
            command.ExtensionOfTime
        );

        // 6. Persist project changes and milestone data to the database
        await context.Expirations.AddRangeAsync(expirations, ct);
        await context.SaveChangesAsync(ct);

        var res = new AddExpirationResponse(
            Id: project.Id,
            Message: "Project is activated successfully. Expirations Added."
        );

        return AppResult<AddExpirationResponse>.Success(res);
    }
}