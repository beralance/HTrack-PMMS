using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;
using PMMS.Server.Common.Endpoints;
using PMMS.Server.Common.Result;
using PMMS.Server.Common.Security;
using PMMS.Server.Infrastructure.Persistence;
using PMMS.Server.Domain.Enums;
using PMMS.Server.Common.Helper;

namespace PMMS.Server.Features.Projects;

public sealed class AddCertificateOfCompletion : IEndpoint
{
    public record Request(
        DateOnly CocDate
    );

    public record Command(
        Guid ProjectId,
        DateOnly CocDate
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
            RuleFor(x => x.CocDate)
                .NotEmpty()
                .LessThanOrEqualTo(_ => DateOnly.FromDateTime(DateTime.Today))
                .WithMessage("Certificate of Completion date cannot be in the future.");
        }
    }

    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapPost("projects/{projectId:guid}/add-certificate-of-completion", 
        async (Guid projectId, Request request, ISender sender, CancellationToken ct) =>
        {
            var result = await sender.Send(new Command(projectId, request.CocDate), ct);
            return result.IsSuccess
                ? TypedResults.Ok(result)
                : result.ToProblem();   
        })
        .WithName("AddCertificateOfCompletion")
        .WithTags("Projects")
        .WithSummary("Add certificate of completion")
        .RequireAuthorization("PermanentOnly")
        .Produces(StatusCodes.Status200OK)
        .ProducesValidationProblem()
        .ProducesProblem(StatusCodes.Status404NotFound)
        .ProducesProblem(StatusCodes.Status409Conflict)
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
                .Include(p => p.Municipality)
                .FirstOrDefaultAsync(p => p.Id == command.ProjectId, ct);

            if (project is null || project.IsDeleted)
            {
                return AppResult<Response>.Failure($"Project {command.ProjectId} was not found.", ErrorType.NotFound);
            }

            if (!userContext.AssignedProvinceIds.Contains(project.Municipality.ProvinceId))
            {
                return AppResult<Response>.Failure(
                    "You do not have permission to execute state mutations for resources in this territory.", 
                    ErrorType.Forbidden);
            }

            if (project.SetupStatus != ProjectSetupStatuses.Active)
            {
                return AppResult<Response>.Failure($"Project {command.ProjectId} is not currently under active expiration tracking.", ErrorType.Conflict);
            }

            if (project.HasCoc == true)
            { 
                return AppResult<Response>.Failure($"Project {command.ProjectId} already has an existing Certificate of Completion record.", ErrorType.Conflict);
            }

            if (command.CocDate < project.DateIssued)
            {
                return AppResult<Response>.Failure($"The COC date cannot be earlier than the project issuance date ({project.DateIssued}).", ErrorType.Conflict);
            }

            var expirations = await context.Expirations
                .Where(e => e.ProjectId == project.Id)
                .ToListAsync(ct);

            bool hasActiveEot = project.IsExtended && project.Status == ProjectStatuses.Extended;

            var activeEotRecord = expirations.FirstOrDefault(e => e.Type == ExpirationTypes.ExtensionOfTime && e.IsActive);
            DateOnly finalTimelineTarget = (hasActiveEot && activeEotRecord != null)
                ? (activeEotRecord.ExpiresOn ?? command.CocDate)
                : command.CocDate;

            var now = DateTimeOffset.UtcNow;
            project.CocDate = command.CocDate;
            project.HasCoc = true;
            project.Status = ProjectStatuses.Completed;
            project.IsModified = true;
            project.ModifiedById = userId ?? string.Empty;
            project.UpdatedAt = now;

            var calculator = new ProjectTimelineCalculator(project.DateIssued);

            foreach (var expiration in expirations)
            {
                if (expiration.Type == ExpirationTypes.SemestralReport || 
                    expiration.Status != ExpirationStatuses.Ongoing)
                {
                    continue;
                }

                switch (expiration.Type)
                {
                    case ExpirationTypes.DateOfCompletion:
                        if (hasActiveEot)
                        {
                            continue;
                        }

                        expiration.Status = ExpirationStatuses.Completed;
                        expiration.CompletedAt = command.CocDate;
                        expiration.LastExpirationDate = expiration.ExpiresOn;
                        expiration.ExpiresOn = null;
                        expiration.UpdatedAt = now;
                        break;

                    case ExpirationTypes.ExtensionOfTime:
                        if (!hasActiveEot || expiration.ExpiresOn == null)
                        {
                            continue;
                        }
                        expiration.Status = ExpirationStatuses.Completed;
                        expiration.CompletedAt = command.CocDate;
                        expiration.LastExpirationDate = expiration.ExpiresOn;
                        expiration.ExpiresOn = null;
                        expiration.UpdatedAt = now;
                        break;

                    case ExpirationTypes.PerformanceBond:
                        DateOnly? finalPbMilestone = calculator.CalculateLastMilestoneBeforeCompletion(finalTimelineTarget, 12);

                        expiration.Status = ExpirationStatuses.Completed;
                        expiration.LastExpirationDate = finalPbMilestone;
                        expiration.CompletedAt = command.CocDate; 
                        expiration.ExpiresOn = null;
                        expiration.UpdatedAt = now;
                        break;
                }
            }

            await context.SaveChangesAsync(ct);

            var res = new Response(
                ProjectId: project.Id,
                Message: "Certificate of Completion added successfully and monitoring metrics closed."
            );

            return AppResult<Response>.Success(res);
        }
    }
}
