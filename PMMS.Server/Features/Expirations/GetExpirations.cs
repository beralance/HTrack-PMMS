using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;
using PMMS.Server.Common.Endpoints;
using PMMS.Server.Common.Result;
using PMMS.Server.Common.Security;
using PMMS.Server.Domain.Enums;
using PMMS.Server.Infrastructure.Persistence;

namespace PMMS.Server.Features.Expirations;

public sealed class GetExpirations : IEndpoint
{
    public record Query(
        int Page = 1,
        int PageSize = 20,
        string? Search = null,
        ExpirationStatuses? Status = null,
        ExpirationTypes? Type = null
    ) : IRequest<AppResult<Response>>;

    public record Response(
        IReadOnlyList<ExpirationQueueItemDto> Items,
        int Page,
        int PageSize,
        int TotalCount,
        string Message
    );

    public record ExpirationQueueItemDto(
        int ExpirationId,
        ExpirationTypes Type,
        ExpirationStatuses Status,
        DateOnly? ExpiresOn,
        Guid ProjectId,
        string ProjectName,
        string Developer,
        ProjectStatuses ProjectStatus,
        string MunicipalityName,
        string ProvinceName
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
        app.MapGet("expirations", async ([AsParameters] Query query, ISender sender, CancellationToken ct) =>
        {
            var result = await sender.Send(query, ct);
            
            return result.IsSuccess
                ? TypedResults.Ok(result)
                : result.ToProblem();
        })
        .WithName("GetExpirations")
        .WithTags("Expirations")
        .WithSummary("Get expirations")
        .RequireAuthorization("PermanentOnly")
        .Produces(StatusCodes.Status200OK)
        .ProducesValidationProblem()
        .ProducesProblem(StatusCodes.Status403Forbidden)
        .ProducesProblem(StatusCodes.Status401Unauthorized);
    }

    public class Handler(
        PmmsDbContext context) : IRequestHandler<Query, AppResult<Response>>
    {
        public async Task<AppResult<Response>> Handle(Query query, CancellationToken ct)
        {

            var queryable = context.Expirations
                .AsNoTracking()
                .Where(e => e.IsActive == true && 
                            e.Project != null && 
                            e.Project.IsDeleted == false &&
                            e.Project.SetupStatus == ProjectSetupStatuses.Active);

            var rawCount = await context.Expirations.CountAsync(ct);
            var allRecords = await context.Expirations.Take(5).ToListAsync(ct);

            Console.WriteLine($"DEBUG: Total records in Expirations table: {rawCount}");
            foreach(var rec in allRecords) {
                Console.WriteLine($"DEBUG: Record ID: {rec.Id}, IsActive: {rec.IsActive}, Status: {rec.Status}");
            }

            if (!string.IsNullOrWhiteSpace(query.Search))
            {
                var search = query.Search.Trim();
                queryable = queryable.Where(e => 
                    EF.Functions.Like(e.Project!.ProjectName, $"%{search}%") || 
                    EF.Functions.Like(e.Project.Developer, $"%{search}%"));
            }

            if (query.Type.HasValue)
            {
                queryable = queryable.Where(e => e.Type == query.Type.Value);
            }

            if (query.Status.HasValue)
            {
                queryable = queryable.Where(e => e.Status == query.Status.Value);
            }

            var totalCount = await queryable.CountAsync(ct);

            Console.WriteLine($"TOTAL Count: {totalCount}");

            var items = await queryable
                .OrderBy(e => e.ExpiresOn)
                .Skip((query.Page - 1) * query.PageSize)
                .Take(query.PageSize)
                .Select(e => new ExpirationQueueItemDto(
                    e.Id,
                    e.Type ?? default,
                    e.Status ?? default,
                    e.ExpiresOn,
                    e.ProjectId ?? Guid.Empty,
                    e.Project!.ProjectName,
                    e.Project.Developer,
                    e.Project.Status ?? default,
                    e.Project.Municipality.MunicipalityName,
                    e.Project.Municipality.Province.ProvinceName
                ))
                .ToListAsync(ct);

            

            Console.WriteLine($"DATA FETCHED: {items}");


            var res = new Response(
                Items: items,
                Page: query.Page,
                PageSize: query.PageSize,
                TotalCount: totalCount,
                Message: "Active expiration deadlines queue retrieved successfully."
            );

            return AppResult<Response>.Success(res);
        }
    }
}