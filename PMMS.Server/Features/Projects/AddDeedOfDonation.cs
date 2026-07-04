using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;
using PMMS.Server.Common.Endpoints;
using PMMS.Server.Common.Result;
using PMMS.Server.Common.Security;
using PMMS.Server.Infrastructure.Persistence;
using PMMS.Server.Domain.Enums;

namespace PMMS.Server.Features.Projects;

public sealed class AddDeedOfDonation : IEndpoint
{
    public record Request(
        DateOnly DodDate
    );

    public record Command(
        Guid ProjectId,
        DateOnly DodDate
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
            RuleFor(x => x.DodDate)
                .NotEmpty()
                .LessThanOrEqualTo(_ => DateOnly.FromDateTime(DateTime.Today))
                .WithMessage("Deed of Donation execution date cannot be in the future.");
        }
    }

    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapPost("projects/{projectId:guid}/add-deed-of-donation", 
        async (Guid projectId, Request request, ISender sender, CancellationToken ct) =>
        {
            var result = await sender.Send(new Command(projectId, request.DodDate), ct);
            return result.IsSuccess
                ? TypedResults.Ok(result)
                : result.ToProblem();   
        })
        .WithName("AddDeedOfDonation")
        .WithTags("Projects")
        .WithSummary("Add deed of donation")
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
                    "You do not have authorization to execute data modifications for projects inside this region.", 
                    ErrorType.Forbidden);
            }

            if (project.HasCoc == false)
            {   
                return AppResult<Response>.Failure($"Project '{project.ProjectName}' must possess a verified Certificate of Completion before processing a Deed of Donation.", ErrorType.Conflict);
            }

            if (project.HasDod == true)
            {
                return AppResult<Response>.Failure($"Project '{project.ProjectName}' already has a registered Deed of Donation record.", ErrorType.Conflict);
            }

            if (command.DodDate < project.CocDate)
            {
                return AppResult<Response>.Failure($"The Deed of Donation date ({command.DodDate}) cannot be earlier than the Certificate of Completion date ({project.CocDate}).", ErrorType.Conflict);
            }

            var now = DateTimeOffset.UtcNow;
            project.DodDate = command.DodDate;
            project.HasDod = true;
            project.Status = ProjectStatuses.FullyCompleted;
            project.IsModified = true;
            project.ModifiedById = userId ?? string.Empty;
            project.UpdatedAt = now;

            var expiration = await context.Expirations
                .FirstOrDefaultAsync(e => e.ProjectId == project.Id && 
                                          e.Type == ExpirationTypes.SemestralReport && 
                                          e.Status == ExpirationStatuses.Ongoing, ct);
            
            if (expiration is not null)
            {
                expiration.Status = ExpirationStatuses.Completed;
                expiration.LastExpirationDate = expiration.ExpiresOn;
                expiration.ExpiresOn = null;
                expiration.CompletedAt = command.DodDate; 
                expiration.UpdatedAt = now.UtcDateTime;
            }

            await context.SaveChangesAsync(ct);

            var res = new Response(
                ProjectId: project.Id,
                Message: "Deed of Donation registered successfully. Project tracking lifecycle is now closed."
            );

            return AppResult<Response>.Success(res);
        }
    }
}
