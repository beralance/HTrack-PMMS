using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;
using PMMS.Server.Common.Endpoints;
using PMMS.Server.Common.Result;
using PMMS.Server.Common.Security;
using PMMS.Server.Infrastructure.Persistence;
using PMMS.Server.Domain.Enums;
using PMMS.Server.Domain.Entities;
using PMMS.Server.Common.Helper;

namespace PMMS.Server.Features.Expirations;

public sealed class ExtendProjectExpiration : IEndpoint
{
    public record Request(DateOnly ExtensionOfTime);

    public record Command(Guid ProjectId, DateOnly ExtensionOfTime) : IRequest<AppResult<Response>>;

    public record Response(Guid ProjectId, string Message);

    public class Validator : AbstractValidator<Command>
    {
        public Validator()
        {
            RuleFor(x => x.ProjectId).NotEmpty();
            RuleFor(x => x.ExtensionOfTime).NotEmpty();
        }
    }

    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapPost("expirations/{projectId:guid}/extend-project", 
        async(Guid projectId, Request request, ISender sender, CancellationToken ct) =>
        {
            var result = await sender.Send(new Command(projectId, request.ExtensionOfTime), ct);
            return result.IsSuccess
                ? TypedResults.Ok(result)
                : result.ToProblem();   
        })
        .WithName("ExtendProjectExpiration")
        .WithTags("Expirations")
        .WithSummary("Extend project expiration")
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
                .FirstOrDefaultAsync(p => p.Id == command.ProjectId, ct);

            if (project is null)
            {
                return AppResult<Response>.Failure($"Project {command.ProjectId} was not found.", ErrorType.NotFound);
            }

            if (project.SetupStatus != ProjectSetupStatuses.Active)
            {
                return AppResult<Response>.Failure($"Project {command.ProjectId} is not yet activated. Add expirations to activate.", ErrorType.Validation);
            }

            if (project.Status != ProjectStatuses.OnGoing)
            {   
                return AppResult<Response>.Failure("Project should be ongoing in order to extend.", ErrorType.Validation);
            }


            var existingExpiration = await context.Expirations
                .FirstOrDefaultAsync(e => 
                    e.ProjectId == command.ProjectId && 
                    e.Type == ExpirationTypes.ExtensionOfTime, ct);

            if (existingExpiration is not null)
            {
                context.Expirations.Remove(existingExpiration);
            }


            var addedExtension = new Expiration
            {
                ProjectId = command.ProjectId,
                Type = ExpirationTypes.ExtensionOfTime,
                ExpiresOn = command.ExtensionOfTime,
                Status = ExpirationStatuses.Ongoing,
                IsActive = true,
                CreatedAt = DateTimeOffset.UtcNow
            };

            await context.Expirations.AddAsync(addedExtension, ct);

            var dateOfCompletion = await context.Expirations
                .FirstOrDefaultAsync(e => 
                    e.ProjectId == command.ProjectId && 
                    e.IsActive == true &&
                    e.Type == ExpirationTypes.DateOfCompletion, ct);

            if (dateOfCompletion is not null) 
            {

                if (command.ExtensionOfTime <= dateOfCompletion.ExpiresOn)
                {
                    return AppResult<Response>.Failure(
                        $"The extension date must be after the current completion date ({dateOfCompletion.ExpiresOn}).", 
                        ErrorType.Validation);
                }
                
                dateOfCompletion.Status = ExpirationStatuses.Completed;
                dateOfCompletion.LastExpirationDate = dateOfCompletion.ExpiresOn;
                dateOfCompletion.ExpiresOn = null;
                dateOfCompletion.CompletedAt = DateOnly.FromDateTime(DateTime.Today);
                dateOfCompletion.UpdatedAt = DateTimeOffset.UtcNow;
            }

            project.IsExtended = true;
            project.Status = ProjectStatuses.Extended;
            project.UpdatedAt = DateTime.UtcNow;
            project.IsModified = true;
            project.ModifiedById = userId;

            var rollingMilestones = await context.Expirations
                .Where(e => e.ProjectId == command.ProjectId && 
                           (e.Type == ExpirationTypes.SemestralReport || e.Type == ExpirationTypes.PerformanceBond) &&
                           e.IsActive)
                .ToListAsync(ct);

            if (rollingMilestones.Count != 0)
            {
                var calculator = new ProjectTimelineCalculator(project.DateIssued);

                foreach (var milestone in rollingMilestones)
                {
                    int interval = milestone.Type == ExpirationTypes.SemestralReport ? 6 : 12;
                    
                    var (nextDate, _) = calculator.CalculateUpcoming(interval);
                    
                    milestone.ExpiresOn = nextDate;
                    milestone.UpdatedAt = DateTimeOffset.UtcNow;
                }
            }
            
            await context.SaveChangesAsync(ct);

            var res = new Response(
                ProjectId: project.Id,
                Message: "Project was extended successfully."
            );

            return AppResult<Response>.Success(res);
        }
    }
}
