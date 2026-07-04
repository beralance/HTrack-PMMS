using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;
using PMMS.Server.Common.Endpoints;
using PMMS.Server.Common.Result;
using PMMS.Server.Domain.Entities;
using PMMS.Server.Domain.Enums;
using PMMS.Server.Infrastructure.Persistence;

namespace PMMS.Server.Features.CentralProjectNetwork;

public sealed class GetCpnProjects : IEndpoint
{
    public record Query(
        int Page = 1,
        int PageSize = 20,
        string? Search = null,
        int? ProvinceId = null,
        int? MunicipalityId = null,
        int? ProjectTypeId = null,
        ProjectStatuses? Status = null
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
        string Barangay,
        ProjectStatuses Status,
        string AddedById,
        string MunicipalityName,
        string ProvinceName,
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
        app.MapGet("/central-project-network", async ([AsParameters] Query query, ISender sender, CancellationToken ct) =>
        {
            var result = await sender.Send(query, ct);
            
            return result.IsSuccess
                ? TypedResults.Ok(result)
                : result.ToProblem();
        })
        .WithName("GetAllProjects")
        .WithTags("Central Project Network")
        .WithSummary("Get CPN Projects")
        .RequireAuthorization("PermanentOnly")
        .Produces(StatusCodes.Status200OK)
        .ProducesValidationProblem()
        .ProducesProblem(StatusCodes.Status422UnprocessableEntity);
    }

    public class Handler(PmmsDbContext context) : IRequestHandler<Query, AppResult<Response>>
    {
        public async Task<AppResult<Response>> Handle(Query query, CancellationToken ct)
        {
            var queryable = context.Projects
                .AsNoTracking()
                .Where(p => !p.IsDeleted && p.SetupStatus == ProjectSetupStatuses.Active);

            if (!string.IsNullOrWhiteSpace(query.Search))
            {
                var search = query.Search.Trim();
                queryable = queryable.Where(p => 
                    EF.Functions.Like(p.ProjectName, $"%{search}%") || 
                    EF.Functions.Like(p.Developer, $"%{search}%") ||
                    EF.Functions.Like(p.LsNo, $"%{search}%"));
            }

            if (query.ProvinceId.HasValue)
            {
                queryable = queryable.Where(p => p.Municipality.ProvinceId == query.ProvinceId.Value);
            }

            if (query.MunicipalityId.HasValue)
            {
                queryable = queryable.Where(p => p.MunicipalityId == query.MunicipalityId.Value);
            }

            if (query.ProjectTypeId.HasValue)
            {
                queryable = queryable.Where(p => p.ProjectTypeId == query.ProjectTypeId.Value);
            }

            if (query.Status.HasValue)
            {
                queryable = queryable.Where(p => p.Status == query.Status.Value);
            }

            var totalCount = await queryable.CountAsync(ct);

            var items = await queryable
                .OrderByDescending(p => p.DateIssued)
                .Skip((query.Page - 1) * query.PageSize)
                .Take(query.PageSize)
                .Select(p => new Dto(
                    p.Id,
                    p.DateIssued,
                    p.ProjectName,
                    p.Developer,
                    p.Barangay,
                    p.Status ?? default,
                    p.AddedById,
                    p.Municipality.MunicipalityName,
                    p.Municipality.Province.ProvinceName,
                    p.ProjectType.Type
                ))
                .ToListAsync(ct);

            var response = new Response(
                Items: items,
                Page: query.Page,
                PageSize: query.PageSize,
                TotalCount: totalCount,
                Message: "Central Project Network data fetched successfully."
            );

            return AppResult<Response>.Success(response);
        }
    }
}