using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;
using PMMS.Server.Common.Endpoints;
using PMMS.Server.Common.Result;
using PMMS.Server.Common.Security;
using PMMS.Server.Domain.Enums;
using PMMS.Server.Infrastructure.Persistence;

namespace PMMS.Server.Features.Expirations;

public sealed class GetExpirationById : IEndpoint
{
    public record Query(int ExpirationId) : IRequest<AppResult<Response>>;

    public record Response(
        ExpirationDetailDto ExpirationItem,
        string Message
    );

    public record ExpirationDetailDto(
        int Id,
        ExpirationTypes Type,
        ExpirationStatuses Status,
        DateOnly? ExpiresOn,
        DateOnly? CompletedAt,
        DateOnly? HandledAt,
        DateTimeOffset? ExpiredAt,
        DateOnly? CancelledAt,
        DateTimeOffset CreatedAt,
        
        Guid ProjectId,
        string ProjectName,
        string Developer,
        string Owner,
        ProjectStatuses ProjectStatus,
        string MunicipalityName,
        string ProvinceName
    );

    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapGet("expirations/{expirationId:int}", async (int expirationId, ISender sender, CancellationToken ct) =>
        {
            var result = await sender.Send(new Query(expirationId), ct);

            return result.IsSuccess
                ? TypedResults.Ok(result)
                : result.ToProblem();
        })
        .WithName("GetExpirationById")
        .WithTags("Expirations")
        .WithSummary("Get expiration by Id")
        .RequireAuthorization("PermanentOnly")
        .Produces(StatusCodes.Status200OK)
        .ProducesValidationProblem()
        .ProducesProblem(StatusCodes.Status403Forbidden)
        .ProducesProblem(StatusCodes.Status404NotFound)
        .ProducesProblem(StatusCodes.Status401Unauthorized);
    }

    public class Validator : AbstractValidator<Query>
    {
        public Validator()
        {
            RuleFor(x => x.ExpirationId).GreaterThan(0);
        }
    }

    public class Handler(
        PmmsDbContext context) : IRequestHandler<Query, AppResult<Response>>
    {
        public async Task<AppResult<Response>> Handle(Query query, CancellationToken ct)
        {
            // 1. Fetch record in a single optimized pass, enforcing security parameters immediately
            var expiration = await context.Expirations
                .AsNoTracking()
                .Where(e => e.Id == query.ExpirationId && 
                            e.IsActive == true && 
                            e.Project != null && 
                            e.Project.IsDeleted == false &&
                            e.Project.SetupStatus == ProjectSetupStatuses.Active)
                .Select(e => new ExpirationDetailDto(
                    e.Id,
                    e.Type ?? default,
                    e.Status ?? default,
                    e.ExpiresOn,
                    e.CompletedAt,
                    e.HandledAt,
                    e.ExpiredAt,
                    e.CancelledAt,
                    e.CreatedAt,
                    
                    e.ProjectId ?? Guid.Empty,
                    e.Project!.ProjectName,
                    e.Project.Developer,
                    e.Project.Owner,
                    e.Project.Status ?? default,
                    e.Project.Municipality.MunicipalityName,
                    e.Project.Municipality.Province.ProvinceName
                ))
                .FirstOrDefaultAsync(ct);

            // 2. If nothing returned, evaluate exactly why the access failed
            if (expiration is null)
            {
                // Verify if the expiration record exists at all in the database
                bool expirationExistsAtAll = await context.Expirations
                    .AnyAsync(e => e.Id == query.ExpirationId && e.IsActive && e.Project != null && !e.Project.IsDeleted, ct);

                if (expirationExistsAtAll)
                {
                    return AppResult<Response>.Failure(
                        "You do not have permissions to access resources in this province.", 
                        ErrorType.Forbidden);
                }

                return AppResult<Response>.Failure(
                    $"Expiration {query.ExpirationId} was not found.", 
                    ErrorType.NotFound);
            }

            // 3. Assemble response payload
            var res = new Response(
                ExpirationItem: expiration,
                Message: "Expiration details fetched successfully."
            );

            return AppResult<Response>.Success(res);
        }
    }
}