using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;
using PMMS.Server.Common.Endpoints;
using PMMS.Server.Common.Result;
using PMMS.Server.Common.Security;
using PMMS.Server.Domain.Enums;
using PMMS.Server.Infrastructure.Persistence;

namespace PMMS.Server.Features.Expirations;

public sealed class GetExpirationsByProjectId : IEndpoint
{
    public record Query(Guid ProjectId) : IRequest<AppResult<Response>>;

    public record Response(
        ProjectHeaderDto Project,
        string Message
    );

    public record ProjectHeaderDto(
        Guid Id,
        string ProjectName,
        string Developer,
        string Owner,
        ProjectStatuses Status,
        string MunicipalityName,
        string ProvinceName,
        IReadOnlyList<ProjectExpirationHistoryDto> Expirations
    );

    public record ProjectExpirationHistoryDto(
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
        app.MapGet("projects/{projectId:guid}/expirations", async (Guid projectId, ISender sender, CancellationToken ct) =>
        {
            var result = await sender.Send(new Query(projectId), ct);

            return result.IsSuccess
                ? TypedResults.Ok(result)
                : result.ToProblem();
        })
        .WithName("GetExpirationsByProject")
        .WithTags("Expirations")
        .WithSummary("Get expirations by project id")
        .RequireAuthorization("PermanentOnly")
        .Produces(StatusCodes.Status200OK)
        .ProducesProblem(StatusCodes.Status403Forbidden)
        .ProducesProblem(StatusCodes.Status404NotFound)
        .ProducesProblem(StatusCodes.Status401Unauthorized);
    }

    public class Handler(PmmsDbContext context) : IRequestHandler<Query, AppResult<Response>>
    {
        public async Task<AppResult<Response>> Handle(Query query, CancellationToken ct)
        {
            

            // 1. Fetch project profile and nest child history elements in a single optimized pass
            var projectWithExpirations = await context.Projects
                .AsNoTracking()
                .Where(p => p.Id == query.ProjectId && 
                            p.IsDeleted == false && 
                            p.SetupStatus == ProjectSetupStatuses.Active) 
                .Select(p => new ProjectHeaderDto(
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
                        .Select(e => new ProjectExpirationHistoryDto(
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
                .FirstOrDefaultAsync(ct);

            // 2. Evaluate query outputs if nothing was returned
            if (projectWithExpirations is null)
            {
                return AppResult<Response>.Failure(
                    $"Project {query.ProjectId} was not found.", 
                    ErrorType.NotFound);
            }

            // 3. Formulate success payload
            var res = new Response(
                Project: projectWithExpirations,
                Message: $"Project '{projectWithExpirations.ProjectName}' along with its complete expiration tracking history loaded successfully."
            );

            return AppResult<Response>.Success(res);
        }
    }
}