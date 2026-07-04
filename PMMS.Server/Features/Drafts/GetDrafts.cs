using FluentValidation;
using Mapster;
using MediatR;
using Microsoft.EntityFrameworkCore;
using PMMS.Server.Common.Endpoints;
using PMMS.Server.Common.Result;
using PMMS.Server.Common.Security;
using PMMS.Server.Domain.Entities;
using PMMS.Server.Infrastructure.Persistence;

namespace PMMS.Server.Features.Drafts;


public sealed class GetDrafts : IEndpoint {
    
    public record Query(
        int PageNumber = 1,
        int PageSize = 20,
        string? Search = null,
        int? MunicipalityId = null,
        int? ProjectTypeId = null,
        bool IncludeDeleted = false
    ) : IRequest<AppResult<Response>>;

    public record Response(
        IReadOnlyList<Dto> Items,
        string Message,
        int TotalCount
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
        int MunicipalityId,
        string MunicipalityName,
        int ProjectTypeId,
        string ProjectType,
        string? Remarks,

        bool? HasCoc,
        bool? HasDod,

        string AddedById,
        DateTimeOffset CreatedAt
    );

    public class Validator : AbstractValidator<Query>
    {
        public Validator()
        {
            RuleFor(x => x.PageNumber).GreaterThan(0);
            RuleFor(x => x.PageSize).InclusiveBetween(1, 100);
        }
    }

    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapGet("drafts", async (
            [AsParameters] Query query,
            ISender sender,
            CancellationToken ct) =>
        {
            var result = await sender.Send(query, ct);

            return result.IsSuccess
                ? TypedResults.Ok(result)
                : result.ToProblem();
        })
        .WithName("GetDrafts")
        .WithTags("Drafts")
        .WithSummary("Get drafts")
        .RequireAuthorization("TemporaryOnly")
        .Produces(StatusCodes.Status200OK)
        .ProducesProblem(StatusCodes.Status401Unauthorized)
        .ProducesProblem(StatusCodes.Status403Forbidden);
    }


    public class Handler(
        PmmsDbContext context, 
        IUserContext userContext)
        : IRequestHandler<Query, AppResult<Response>>
    {
        public async Task<AppResult<Response>> Handle(Query query, CancellationToken ct)
        {
            var userId = userContext.UserId;
            if (userId is null)
            {
                return AppResult<Response>.Failure("User identity could not be verified.", ErrorType.Unauthorized);
            }

            var drafts = context.Drafts
                .AsNoTracking()
                .Where(d => d.AddedById == userId && d.IsPublished == false);

            if (!string.IsNullOrWhiteSpace(query.Search))
            {
                var search = query.Search.Trim().ToLower();
                drafts = drafts.Where(d =>
                    d.ProjectName.ToLower().Contains(search) ||
                    d.Developer.ToLower().Contains(search));
            }

            if (query.MunicipalityId.HasValue)
                drafts = drafts.Where(d => d.MunicipalityId == query.MunicipalityId.Value);

            if (query.ProjectTypeId.HasValue)
                drafts = drafts.Where(d => d.ProjectTypeId == query.ProjectTypeId.Value);

            var totalCount = await drafts.CountAsync(ct);

            var items = await drafts
                .OrderByDescending(d => d.CreatedAt)
                .Skip((query.PageNumber - 1) * query.PageSize)
                .Take(query.PageSize)
                .Select(d => new Dto(
                    Id: d.Id,
                    DateIssued: d.DateIssued,
                    ProjectName: d.ProjectName,
                    Developer: d.Developer,
                    Owner: d.Owner,
                    CrNo: d.CrNo,
                    LsNo: d.LsNo,
                    Barangay: d.Barangay,
                    Salable: d.Salable,
                    IsFm: d.IsFm,
                    MunicipalityId: d.MunicipalityId,
                    MunicipalityName: d.Municipality.MunicipalityName,
                    ProjectTypeId: d.ProjectTypeId,
                    ProjectType: d.ProjectType.Type,
                    Remarks: d.Remarks,

                    HasCoc: d.HasCoc,
                    HasDod: d.HasDod,

                    AddedById: d.AddedById,
                    CreatedAt: d.CreatedAt
                ))
                .ToListAsync(ct);

            return AppResult<Response>.Success(new Response(
                Items: items,
                Message: "Drafts successfully fetched.",
                TotalCount: totalCount
            ));
        }
    }
}
