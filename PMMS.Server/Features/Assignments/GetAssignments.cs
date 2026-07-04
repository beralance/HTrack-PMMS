using FluentValidation;
using Mapster;
using MediatR;
using Microsoft.EntityFrameworkCore;
using PMMS.Server.Common.Endpoints;
using PMMS.Server.Common.Result;
using PMMS.Server.Infrastructure.Persistence;

namespace PMMS.Server.Features.Assignments;


public sealed class GetAssignment : IEndpoint {

    public record Query(
        int Page = 1,
        int PageSize = 20,
        string? Search = null,
        int? ProvinceId = null
    ) : IRequest<AppResult<Response>>;

    public record Response(
        IReadOnlyList<Dto> Items,
        string Message,
        int Page,
        int PageSize,
        int TotalCount);

    public record Dto(
        string UserId,
        string UserName,
        string Email,
        int ProvinceId,
        string ProvinceName,
        DateTimeOffset CreatedAt);

    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapGet("assignments", async([AsParameters] Query query, ISender sender, CancellationToken ct) =>
            {
                var result = await sender.Send(query, ct);

                return result.IsSuccess
                    ? TypedResults.Ok(result)
                    : result.ToProblem();
            })
            .WithName("GetAssignments")
            .WithTags("Assignments")
            .WithSummary("Get Assignments")
            .RequireAuthorization("AdminOnly")
            .Produces<Response>(StatusCodes.Status200OK);
    }


    public class Handler(PmmsDbContext context) 
        : IRequestHandler<Query, AppResult<Response>>
    {
        public async Task<AppResult<Response>> Handle(Query query, CancellationToken ct)
        {
            var assignments = context.Assignments
                .AsNoTracking()
                .Include(a => a.User)
                .Include(a => a.Province)
                .Where(a => a.User != null && a.User.IsDeleted == false);

            if (query.ProvinceId.HasValue)
                assignments = assignments.Where(a => a.ProvinceId == query.ProvinceId);

            if (!string.IsNullOrWhiteSpace(query.Search))
            {
                var search = query.Search.Trim().ToLower();
                assignments = assignments.Where(a =>
                    a.User!.UserName!.Contains(search, StringComparison.CurrentCultureIgnoreCase) ||
                    a.User.Email!.Contains(search, StringComparison.CurrentCultureIgnoreCase));
            }

            var totalCount = await assignments.CountAsync(ct);

            var items = await assignments
                .OrderBy(a => a.User!.UserName)
                .ThenBy(a => a.Province.ProvinceName)
                .Skip((query.Page - 1) * query.PageSize)
                .Take(query.PageSize)
                .Select(a => new Dto(
                    a.UserId!,
                    a.User!.UserName ?? "Unknown",
                    a.User.Email ?? string.Empty,
                    a.ProvinceId,
                    a.Province.ProvinceName,
                    a.CreatedAt
                ))
                .ToListAsync(ct);
            
            var res = new Response(
                Items: items,
                Message: "Assignment list are fetched successfully.",
                Page: query.Page,
                PageSize:  query.PageSize,
                TotalCount: totalCount
            );

            return AppResult<Response>.Success(res);
        }
    }
}