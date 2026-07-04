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
        var project = await context.Projects
            .Include(p => p.Municipality)
            .FirstOrDefaultAsync(p => p.Id == command.Id, ct);

        if (project is null)
        {
            return AppResult<ResetExpirationResponse>.Failure($"Project {command.Id} does not exist.", ErrorType.NotFound);
        }

        var userProvinces = userContext.AssignedProvinceIds;
        if (!userProvinces.Contains(project.Municipality.ProvinceId))
        {
            return AppResult<ResetExpirationResponse>.Failure(
                "You do not have permissions to modify projects in this province.", 
                ErrorType.Forbidden
            );
        }

        var projectStatus = command switch
        {
            { HasCoc: true, HasDod: true } => ProjectStatuses.FullyCompleted,
            { HasCoc: true } => ProjectStatuses.Completed,
            { ExtensionOfTime: not null } => ProjectStatuses.Extended,
            _ => ProjectStatuses.OnGoing
        };

        project.CocDate = command.CocDate;
        project.HasCoc = command.HasCoc;
        project.DodDate = command.DodDate;
        project.HasDod = command.HasDod;
        project.Status = projectStatus;
        project.IsExtended = command.ExtensionOfTime.HasValue;
        
        project.IsModified = true;
        project.UpdatedAt = DateTimeOffset.UtcNow;
        project.ModifiedById = userContext.UserId;

        var existingExpirations = await context.Expirations
            .Where(e => e.ProjectId == command.Id)
            .ToListAsync(ct);

        if (existingExpirations.Count != 0)
        {
            context.Expirations.RemoveRange(existingExpirations);
        }

        var expirations = CreateExpirationBehavior.CalculateExpirations(
            project,
            projectStatus,
            command.DateOfCompletion,
            command.ExtensionOfTime
        );
        
        await context.Expirations.AddRangeAsync(expirations, ct);
        await context.SaveChangesAsync(ct);

        var res = new ResetExpirationResponse(
            Id: project.Id,
            Message: "Project updated and expirations recalculated successfully."
        );

        return AppResult<ResetExpirationResponse>.Success(res);
    }
}