using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;
using PMMS.Server.Common.Endpoints;
using PMMS.Server.Common.Result;
using PMMS.Server.Common.Security;
using PMMS.Server.Domain.Enums;
using PMMS.Server.Infrastructure.Persistence;

namespace PMMS.Server.Features.Projects;

public sealed class GetProjects : IEndpoint {

    public record Query(
        int Page = 1,
        int PageSize = 20,
        string? Search = null,
        int? ProvinceId = null
    ) : IRequest<AppResult<Response>>;

    public record Response(
        IReadOnlyList<Dto> Items,
        int Page,
        int PageSize,
        int TotalCount,
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
        ProjectStatuses Status,
        string? Remarks,
        string AddedById,
        int MunicipalityId,
        string MunicipalityName,
        int ProjectTypeId,
        string ProjectType
    );

    public class Validator : AbstractValidator<Query>
    {
        public Validator()
        {
            RuleFor(x => x.Page).GreaterThan(0);
            RuleFor(x => x.PageSize).InclusiveBetween(1, 100);
        }
    }

    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapGet("projects", async ([AsParameters] Query query, ISender sender, CancellationToken ct) =>
            {
                var result = await sender.Send(query, ct);
                
                return result.IsSuccess
                    ? TypedResults.Ok(result)
                    : result.ToProblem();
            })
            .WithName("GetProjects")
            .WithTags("Projects")
            .WithSummary("Get projects")
            .RequireAuthorization("PermanentOnly")
            .Produces(StatusCodes.Status200OK)
            .ProducesValidationProblem()
            .ProducesProblem(StatusCodes.Status403Forbidden)
            .ProducesProblem(StatusCodes.Status422UnprocessableEntity);
    }

    public class Handler(
        PmmsDbContext context,
        IUserContext userContext) : IRequestHandler<Query, AppResult<Response>>
    {
        public async Task<AppResult<Response>> Handle(Query query, CancellationToken ct)
        {
            var assignedProvinces = userContext.AssignedProvinceIds;

            // 1. Core Base Query: Scope data exclusively to non-deleted active records inside user boundaries
            var queryable = context.Projects
                .AsNoTracking()
                .Where(p => !p.IsDeleted && 
                            assignedProvinces.Contains(p.Municipality.ProvinceId));

            // 2. Search Filter Execution
            if (!string.IsNullOrWhiteSpace(query.Search))
            {
                var search = query.Search.Trim();
                queryable = queryable.Where(p => 
                    EF.Functions.Like(p.ProjectName, $"%{search}%") || 
                    EF.Functions.Like(p.Developer, $"%{search}%") ||
                    EF.Functions.Like(p.LsNo, $"%{search}%"));
            }

            // 3. Province Filter Integration
            if (query.ProvinceId.HasValue)
            {
                if (!assignedProvinces.Contains(query.ProvinceId.Value))
                {
                    return AppResult<Response>.Failure(
                        "You do not have authorization to view projects inside the requested province filter.", 
                        ErrorType.Forbidden);
                }

                queryable = queryable.Where(p => p.Municipality.ProvinceId == query.ProvinceId.Value);
            }

            // 4. Calculate Total Matching Item Counter
            var totalCount = await queryable.CountAsync(ct);

            // 5. Execute Efficient Pagination Window & Flat Selective Mapping Projection
            var items = await queryable
                .OrderByDescending(p => p.DateIssued)
                .Skip((query.Page - 1) * query.PageSize)
                .Take(query.PageSize)
                .Select(p => new Dto(
                    p.Id,
                    p.DateIssued,
                    p.ProjectName,
                    p.Developer,
                    p.Owner,
                    p.CrNo,
                    p.LsNo,
                    p.Barangay,
                    p.Salable,
                    p.IsFm,
                    p.Status ?? default,
                    p.Remarks,
                    p.AddedById,
                    p.MunicipalityId,
                    p.Municipality.MunicipalityName,
                    p.ProjectTypeId,
                    p.ProjectType.Type
                ))
                .ToListAsync(ct);

            var res = new Response(
                Items: items,
                Page: query.Page,
                PageSize: query.PageSize,
                TotalCount: totalCount,
                Message: "Projects fetched successfully."
            );

            return AppResult<Response>.Success(res);
        }
    }
}