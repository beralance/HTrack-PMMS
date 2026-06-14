using Mapster;
using MediatR;
using Microsoft.EntityFrameworkCore;
using PMMS.Server.Common.Endpoints;
using PMMS.Server.Common.Result;
using PMMS.Server.Common.Security;
using PMMS.Server.Infrastructure.Persistence;

namespace PMMS.Server.Features.Projects;

public sealed class GetProjectById : IEndpoint {

    public record Query(Guid Id) : IRequest<AppResult<Response>>;

    public record Response(
        Dto Project,
        string Message
    );

    public record Dto(
        Guid Id,
        DateOnly DateIssued,
        string ProjectName,
        string Developer,
        string Owner,
        string CrNo,
        string LsNo,
        string Barangay,
        string Salable,
        bool IsFm,
        bool? HasCoc,
        bool? HasDod,
        DateOnly? CocDate,
        DateOnly? DodDate,
        string? Remarks,
        string ProjectTypeName,
        string MunicipalityName,
        string ProvinceName
    );

    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapGet("projects/{id:guid}", async (Guid id, ISender sender, CancellationToken ct) =>
            {
                var result = await sender.Send(new Query(id), ct);
                
                return result.IsSuccess
                    ? TypedResults.Ok(result)
                    : result.ToProblem();
            })
            .WithName("GetProjectById")
            .WithTags("Projects")
            .WithSummary("Get project by id")
            .RequireAuthorization("PermanentOnly") 
            .Produces(StatusCodes.Status200OK)
            .ProducesProblem(StatusCodes.Status404NotFound)
            .ProducesProblem(StatusCodes.Status403Forbidden)
            .ProducesProblem(StatusCodes.Status401Unauthorized);
    }

    public class Handler(
        PmmsDbContext context,
        IUserContext userContext) : IRequestHandler<Query, AppResult<Response>>
    {
        public async Task<AppResult<Response>> Handle(Query query, CancellationToken ct)
        {
            var assignedProvinces = userContext.AssignedProvinceIds;

            // 1. Fetch Project with inline security constraints to save performance trips
            var project = await context.Projects
                .AsNoTracking()
                .Include(p => p.ProjectType)
                .Include(p => p.Municipality)
                    .ThenInclude(m => m.Province)
                .Where(p => p.Id == query.Id && !p.IsDeleted && assignedProvinces.Contains(p.Municipality.ProvinceId))
                .FirstOrDefaultAsync(ct);

            // 2. If null, verify if it's missing completely or just walled off by security permissions
            if (project == null)
            {
                var existsGlobally = await context.Projects
                    .AnyAsync(p => p.Id == query.Id && !p.IsDeleted, ct);

                if (existsGlobally)
                {
                    return AppResult<Response>.Failure(
                        "You do not have authorization to view projects inside this geographical territory.", 
                        ErrorType.Forbidden);
                }

                return AppResult<Response>.Failure($"Project {query.Id} was not found.", ErrorType.NotFound);
            }

            // 3. Map manually or via Mapster to your flat DTO shape safely
            var projectDto = new Dto(
                project.Id,
                project.DateIssued,
                project.ProjectName,
                project.Developer,
                project.Owner,
                project.CrNo,
                project.LsNo,
                project.Barangay,
                project.Salable,
                project.IsFm,
                project.HasCoc,
                project.HasDod,
                project.CocDate,
                project.DodDate,
                project.Remarks,
                project.ProjectType?.Type ?? "Unknown",
                project.Municipality?.MunicipalityName ?? "Unknown",
                project.Municipality?.Province?.ProvinceName ?? "Unknown"
            );
            
            var res = new Response(
                Project: projectDto,
                Message: "Project details loaded successfully."
            );

            return AppResult<Response>.Success(res);
        }
    }
}