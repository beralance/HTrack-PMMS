using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;
using PMMS.Server.Common.Endpoints;
using PMMS.Server.Common.Result;
using PMMS.Server.Common.Security;
using PMMS.Server.Infrastructure.Persistence;
using PMMS.Server.Domain.Enums;
using PMMS.Server.Domain.Entities;

namespace PMMS.Server.Features.Expirations;

public sealed class HandlePerformanceBond : IEndpoint
{
    public record Command(
        Guid ProjectId
    ) : IRequest<AppResult<Response>>;

    public record Response(
        Guid ProjectId,
        string Message
    );

    public class Validator : AbstractValidator<Command>
    {
        public Validator()
        {
            RuleFor(x => x.ProjectId).NotEmpty();
        }
    }

    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapPost("expirations/{projectId:guid}/performance-bond", 
        async (Guid projectId, ISender sender, CancellationToken ct) =>
        {
            var result = await sender.Send(new Command(projectId), ct);
            return result.IsSuccess
                ? TypedResults.Ok(result)
                : result.ToProblem();   
        })
        .WithName("HandlePerformanceBond")
        .WithTags("Expirations")
        .WithSummary("Handle performance bond")
        .RequireAuthorization("PermanentOnly")
        .Produces(StatusCodes.Status200OK)
        .ProducesValidationProblem()
        .ProducesProblem(StatusCodes.Status404NotFound)
        .ProducesProblem(StatusCodes.Status409Conflict)
        .ProducesProblem(StatusCodes.Status400BadRequest)
        .ProducesProblem(StatusCodes.Status403Forbidden)
        .ProducesProblem(StatusCodes.Status401Unauthorized)
        .ProducesProblem(StatusCodes.Status422UnprocessableEntity);
    }

    public class Handler(PmmsDbContext context, IUserContext userContext) : IRequestHandler<Command, AppResult<Response>>
    {
        public async Task<AppResult<Response>> Handle(Command command, CancellationToken ct)
        {
            var userId = userContext.UserId;

            var project = await context.Projects
                .Where(p => p.Id == command.ProjectId)
                .FirstOrDefaultAsync(ct);

            if (project is null)
            {
                return AppResult<Response>.Failure($"Project {command.ProjectId} was not found.", ErrorType.NotFound);
            }

            if (project.Status == ProjectStatuses.Cancelled || 
                project.Status == ProjectStatuses.FullyCompleted)
            {
                return AppResult<Response>.Failure($"Cannot handle Performance Bond because the project is already {project.Status}.", ErrorType.Conflict);
            }

            if (project.SetupStatus != ProjectSetupStatuses.Active)
            {
                return AppResult<Response>.Failure($"Project {command.ProjectId} is not active.");
            }

            var performanceBond = await context.Expirations
                .FirstOrDefaultAsync(e => 
                    e.ProjectId == command.ProjectId && 
                    e.Type == ExpirationTypes.PerformanceBond, ct);

            if (performanceBond is null)
            {
                return AppResult<Response>.Failure("Performance Bond expiration was not found for this project.", ErrorType.NotFound);
            }

            ExpirationStatuses?[] allowedStatuses =
            [
                ExpirationStatuses.Ongoing, 
                ExpirationStatuses.Extended, 
                ExpirationStatuses.NearExpiration, 
                ExpirationStatuses.Expired 
            ];

            if (!allowedStatuses.Contains(performanceBond.Status))
            {
                return AppResult<Response>.Failure($"Performance bond cannot be updated. Current status is '{performanceBond.Status}'.");
            }

            if (!performanceBond.ExpiresOn.HasValue)
            {
                return AppResult<Response>.Failure("Cannot renew a Performance Bond that does not have an initial expiration date.");
            }

            DateOnly projectEndDate = project.IsExtended && project.Status == ProjectStatuses.Extended
                ? (await context.Expirations.FirstOrDefaultAsync(e => e.ProjectId == command.ProjectId && e.Type == ExpirationTypes.ExtensionOfTime, ct))?.ExpiresOn 
                    ?? DateOnly.FromDateTime(DateTime.Today)
                : (await context.Expirations.FirstOrDefaultAsync(e => e.ProjectId == command.ProjectId && e.Type == ExpirationTypes.DateOfCompletion, ct))?.CompletedAt 
                    ?? DateOnly.FromDateTime(DateTime.Today);

            DateOnly nextPbDate = performanceBond.ExpiresOn.Value.AddMonths(12);

            if (nextPbDate >= projectEndDate)
            {
                performanceBond.LastExpirationDate = performanceBond.ExpiresOn;
                performanceBond.ExpiresOn = null;
                performanceBond.CompletedAt = projectEndDate;
                performanceBond.Status = ExpirationStatuses.Completed;
            }
            else
            {
                performanceBond.LastExpirationDate = performanceBond.ExpiresOn;
                performanceBond.ExpiresOn = nextPbDate;
                performanceBond.Status = ExpirationStatuses.Ongoing;
            }

            performanceBond.UpdatedAt = DateTimeOffset.UtcNow;
            performanceBond.HandledAt = DateOnly.FromDateTime(DateTime.Today);
            performanceBond.HandledById = userId;
            performanceBond.ExpiredAt = null;

            await context.SaveChangesAsync(ct);

            var res = new Response(
                ProjectId: project.Id,
                Message: performanceBond.Status == ExpirationStatuses.Completed
                    ? "Performance Bond tracking completed successfully. No further bond renewals required for this project timeline."
                    : "Performance Bond handled and successfully extended for another 12 months."
            );

            return AppResult<Response>.Success(res);
        }
    }
}