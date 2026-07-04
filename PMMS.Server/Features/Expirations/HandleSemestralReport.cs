using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;
using PMMS.Server.Common.Endpoints;
using PMMS.Server.Common.Result;
using PMMS.Server.Common.Security;
using PMMS.Server.Infrastructure.Persistence;
using PMMS.Server.Domain.Enums;

namespace PMMS.Server.Features.Expirations;

public sealed class HandleSemestralReport : IEndpoint
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
        app.MapPost("expirations/{projectId:guid}", async (
            Guid projectId, 
            ISender sender, 
            CancellationToken ct) =>
        {
            var result = await sender.Send(new Command(projectId), ct);
            return result.IsSuccess
                ? TypedResults.Ok(result)
                : result.ToProblem();   
        })
        .WithName("HandleSemestralReport")
        .WithTags("Expirations")
        .WithSummary("Handle semestral report")
        .RequireAuthorization("PermanentOnly")
        .Produces(StatusCodes.Status200OK)
        .ProducesValidationProblem()
        .ProducesProblem(StatusCodes.Status404NotFound)
        .ProducesProblem(StatusCodes.Status409Conflict)
        .ProducesProblem(StatusCodes.Status400BadRequest);
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
                return AppResult<Response>.Failure($"Cannot handle Semestral Report because the project is already {project.Status}.", ErrorType.Conflict);
            }

            if (project.SetupStatus != ProjectSetupStatuses.Active)
            {
                return AppResult<Response>.Failure($"Project {command.ProjectId} is not active.");
            }

            var semestralReport = await context.Expirations
                .FirstOrDefaultAsync(e => 
                    e.ProjectId == command.ProjectId && 
                    e.Type == ExpirationTypes.SemestralReport, ct);

            if (semestralReport is null)
            {
                return AppResult<Response>.Failure($"Semestral Report was not found for this project.", ErrorType.NotFound);
            }

            ExpirationStatuses?[] allowedStatuses =
            [
                ExpirationStatuses.Ongoing, 
                ExpirationStatuses.Extended, 
                ExpirationStatuses.NearExpiration, 
                ExpirationStatuses.Expired 
            ];

            if (!allowedStatuses.Contains(semestralReport.Status))
            {
                return AppResult<Response>.Failure($"Semestral Report cannot be updated. Current status is '{semestralReport.Status}'.");
            }

            if (!semestralReport.ExpiresOn.HasValue)
            {
                return AppResult<Response>.Failure($"Cannot renew a Semestral Report that does not have an initial expiration date.");
            }

            DateOnly projectEndDate = project.IsExtended && project.Status == ProjectStatuses.Extended
                ? (await context.Expirations.FirstOrDefaultAsync(e => e.ProjectId == command.ProjectId && e.Type == ExpirationTypes.ExtensionOfTime, ct))?.ExpiresOn 
                    ?? DateOnly.FromDateTime(DateTime.Today)
                : (await context.Expirations.FirstOrDefaultAsync(e => e.ProjectId == command.ProjectId && e.Type == ExpirationTypes.DateOfCompletion, ct))?.CompletedAt 
                    ?? DateOnly.FromDateTime(DateTime.Today);

            DateOnly nextSrDate = semestralReport.ExpiresOn.Value.AddMonths(6);

            if (nextSrDate >= projectEndDate)
            {
                semestralReport.LastExpirationDate = semestralReport.ExpiresOn;
                semestralReport.ExpiresOn = null; 
                semestralReport.CompletedAt = projectEndDate;
                semestralReport.Status = ExpirationStatuses.Completed; 
            }
            else
            {
                semestralReport.LastExpirationDate = semestralReport.ExpiresOn;
                semestralReport.ExpiresOn = nextSrDate;
                semestralReport.Status = ExpirationStatuses.Ongoing;
            }

            semestralReport.UpdatedAt = DateTimeOffset.UtcNow;
            semestralReport.HandledAt = DateOnly.FromDateTime(DateTime.Today);
            semestralReport.ExpiredAt = null;

            await context.SaveChangesAsync(ct);

            var res = new Response(
                ProjectId: project.Id,
                Message: semestralReport.Status == ExpirationStatuses.Completed
                    ? "Semestral Report tracking completed successfully. No further report renewals required for this project timeline."
                    : "Semestral Report handled and successfully extended for another 6 months."
            );

            return AppResult<Response>.Success(res);
        }
    }
}