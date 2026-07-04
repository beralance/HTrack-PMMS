using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;
using PMMS.Server.Common.Endpoints;
using PMMS.Server.Common.Result;
using PMMS.Server.Common.Security;
using PMMS.Server.Domain.Enums;
using PMMS.Server.Infrastructure.Persistence;

namespace PMMS.Server.Features.Expirations;

public sealed class GetExpirationsByProjects : IEndpoint
{
    public record Query() : IRequest<AppResult<Response>>;

    public record Response(
        IReadOnlyList<ProjectWithNestedExpirationsDto> Projects,
        string Message
    );

    public record ProjectWithNestedExpirationsDto(
        Guid Id,
        string ProjectName,
        string Developer,
        string Owner,
        ProjectStatuses Status,
        string MunicipalityName,
        string ProvinceName,
        IReadOnlyList<ProjectExpirationTimelineDto> Expirations
    );

    public record ProjectExpirationTimelineDto(
        int ExpirationId,
        ExpirationTypes Type,
        ExpirationStatuses Status,
        DateOnly? ExpiresOn,
        DateOnly? CompletedAt,
        DateOnly? HandledAt,
        DateTimeOffset? ExpiredAt,
        DateOnly? CancelledAt,
        DateTimeOffset CreatedAt,
        string? HandledByName,
        string? CancelledByName
    );

    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapGet("projects/expirations", async (ISender sender, CancellationToken ct) =>
        {
            var result = await sender.Send(new Query(), ct);

            return result.IsSuccess
                ? TypedResults.Ok(result)
                : result.ToProblem();
        })
        .WithName("GetExpirationsByProjects")
        .WithTags("Expirations")
        .WithSummary("Get expirations by projects")
        .RequireAuthorization("PermanentOnly")
        .Produces(StatusCodes.Status200OK)
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

            var projectsWithExpirations = await context.Projects
                .AsNoTracking()
                .Where(p => p.IsDeleted == false && 
                    p.SetupStatus == ProjectSetupStatuses.Active &&
                    assignedProvinces.Contains(p.Municipality.ProvinceId))
                .OrderBy(p => p.ProjectName)
                .Select(p => new ProjectWithNestedExpirationsDto(
                    p.Id,
                    p.ProjectName,
                    p.Developer,
                    p.Owner,
                    p.Status ?? default,
                    p.Municipality.MunicipalityName,
                    p.Municipality.Province.ProvinceName,
                    
                    context.Expirations
                        .Where(e => e.ProjectId == p.Id && e.IsActive == true)
                        .OrderBy(e => e.Type)
                        .ThenByDescending(e => e.CreatedAt)
                        .Select(e => new ProjectExpirationTimelineDto(
                            e.Id,
                            e.Type ?? default,
                            e.Status ?? default,
                            e.ExpiresOn,
                            e.CompletedAt,
                            e.HandledAt,
                            e.ExpiredAt,
                            e.CancelledAt,
                            e.CreatedAt,
                            e.HandledBy != null ? e.HandledBy.UserName : null,
                            e.CancelledBy != null ? e.CancelledBy.UserName : null
                        ))
                        .ToList()
                ))
                .ToListAsync(ct);


            var res = new Response(
                Projects: projectsWithExpirations,
                Message: $"Successfully retrieved {projectsWithExpirations.Count} regional projects with nested tracking items."
            );

            return AppResult<Response>.Success(res);
        }
    }
}