using MediatR;
using Microsoft.EntityFrameworkCore;
using PMMS.Server.Common.Endpoints;
using PMMS.Server.Common.Result;
using PMMS.Server.Common.Security;
using PMMS.Server.Infrastructure.Persistence;

namespace PMMS.Server.Features.Projects;

public sealed class DeleteProject : IEndpoint {

    public record Command(Guid ProjectId) : IRequest<AppResult<Response>>;

    public record Response(
        Guid Id,
        string Message);

    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapDelete("projects/{projectId:guid}", async (Guid projectId, ISender sender, CancellationToken ct) =>
            {
                var result = await sender.Send(new Command(projectId), ct);

                return result.IsSuccess
                    ? TypedResults.Ok(result)
                    : result.ToProblem();
            })
            .WithName("DeleteProject")
            .WithTags("Projects")
            .WithSummary("Delete project")
            .RequireAuthorization("PermanentOnly")
            .Produces(StatusCodes.Status200OK)
            .ProducesProblem(StatusCodes.Status404NotFound)
            .ProducesProblem(StatusCodes.Status403Forbidden)
            .ProducesProblem(StatusCodes.Status401Unauthorized);
    }

    public class Handler(
        PmmsDbContext context,
        IUserContext userContext) : IRequestHandler<Command, AppResult<Response>>
    {
        public async Task<AppResult<Response>> Handle(Command command, CancellationToken ct)
        {
            var userId = userContext.UserId;

            // 1. Fetch Project based on given Id with necessary relationship data
            var project = await context.Projects
                .Include(p => p.Municipality)
                .FirstOrDefaultAsync(p => p.Id == command.ProjectId, ct);

            // 2. Validate existence and previous soft-delete mutations
            if (project == null || project.IsDeleted)
            {
                return AppResult<Response>.Failure($"Project {command.ProjectId} was not found.", ErrorType.NotFound);
            }

            // 3. Verify security access boundaries against user regional assignments
            if (!userContext.AssignedProvinceIds.Contains(project.Municipality.ProvinceId))
            {
                return AppResult<Response>.Failure(
                    "You do not have permission to delete projects in this region.", 
                    ErrorType.Forbidden);
            }

            // 4. Perform Parent Soft Delete
            var now = DateTimeOffset.UtcNow;
            project.IsDeleted = true;
            project.DeletedById = userId;
            project.DeletedAt = now;
            project.UpdatedAt = now;

            // 5. EF Core Bulk Update: Cascade deactivation directly to database engine without row loading
            await context.Expirations
                .Where(e => e.ProjectId == command.ProjectId && e.IsActive)
                .ExecuteUpdateAsync(setters => setters
                    .SetProperty(e => e.IsActive, false)
                    .SetProperty(e => e.UpdatedAt, now), 
                    ct);

            // 6. Commit the parent project changes
            await context.SaveChangesAsync(ct);

            var res = new Response(
                Id: project.Id,
                Message: "Project was deleted successfully."  
            );

            return AppResult<Response>.Success(res);
        }
    }
}