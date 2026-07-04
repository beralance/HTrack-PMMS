using FluentValidation;
using Mapster;
using MediatR;
using Microsoft.EntityFrameworkCore;
using PMMS.Server.Common.Endpoints;
using PMMS.Server.Common.Result;
using PMMS.Server.Common.Security;
using PMMS.Server.Infrastructure.Persistence;

namespace PMMS.Server.Features.Drafts;


public sealed class CreateDraft : IEndpoint{

    public record Command(
        DateOnly DateIssued,
        string ProjectName,
        string Developer,
        string Owner,
        string CrNo,
        string LsNo,
        string Barangay,
        string Salable,
        bool IsFm,
        string? Remarks,
        int MunicipalityId,
        int ProjectTypeId,
        DateOnly? CocDate,
        DateOnly? DodDate,
        DateOnly DateOfCompletion,
        DateOnly? ExtensionOfTime
    ) : IRequest<AppResult<Response>>;


    public record Response(
        Guid Id,
        string Message
    );


    public class Validator : AbstractValidator<Command>
    {
        public Validator()
        {
            RuleFor(x => x.DateIssued).NotEmpty();
            RuleFor(x => x.ProjectName).NotEmpty().MaximumLength(100);
            RuleFor(x => x.Developer).NotEmpty().MaximumLength(100);
            RuleFor(x => x.Owner).NotEmpty().MaximumLength(100);
            RuleFor(x => x.Owner).NotEmpty().MaximumLength(100);
            RuleFor(x => x.CrNo).NotEmpty().MaximumLength(20);
            RuleFor(x => x.LsNo).NotEmpty().MaximumLength(20);
            RuleFor(x => x.Barangay).NotEmpty().MaximumLength(100);
            RuleFor(x => x.Salable).NotEmpty();
            RuleFor(x => x.MunicipalityId).NotEmpty().GreaterThan(0);
            RuleFor(x => x.ProjectTypeId).NotEmpty().GreaterThan(0);
            RuleFor(x => x.DateOfCompletion).NotNull().WithMessage("Date of completion is required.");
        }
    }


    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapPost("drafts", async (Command command, ISender sender, CancellationToken ct) =>
            {
                var result = await sender.Send(command, ct);

                return result.IsSuccess
                    ? TypedResults.Created($"/drafts/{result.Value?.Id}", result)
                    : result.ToProblem();
            })
            .WithName("CreateDraft")
            .WithTags("Drafts")
            .WithSummary("Create draft")
            .RequireAuthorization("TemporaryOnly")
            .ProducesValidationProblem()
            .Produces(StatusCodes.Status201Created)
            .ProducesProblem(StatusCodes.Status401Unauthorized)
            .ProducesProblem(StatusCodes.Status409Conflict)
            .ProducesProblem(StatusCodes.Status422UnprocessableEntity);
    }


    public class Handler(PmmsDbContext context, IUserContext userContext)
        : IRequestHandler<Command, AppResult<Response>>
    {
        public async Task<AppResult<Response>> Handle(Command command, CancellationToken ct)
        {
            var userId = userContext.UserId;
            if (userId is null)
                return AppResult<Response>.Failure("User context is missing.", ErrorType.Unauthorized);

            var nameNormalized = command.ProjectName.Trim().ToLower();
            
            var existsInDrafts = await context.Drafts
                .AnyAsync(d => d.ProjectName.ToLower() == nameNormalized && d.IsPublished == false, ct);
            
            var existsInProjects = await context.Projects
                .AnyAsync(p => p.ProjectName.ToLower() == nameNormalized && !p.IsDeleted, ct);

            if (existsInDrafts || existsInProjects)
            {
                return AppResult<Response>.Failure($"A project or draft named '{command.ProjectName}' already exists.", ErrorType.Conflict);
            }

            if (command.DodDate.HasValue && command.DodDate.Value < command.DateIssued)
            {
                return AppResult<Response>.Failure("DOD date cannot precede the Date Issued.", ErrorType.Validation);
            }

            var draft = new Domain.Entities.Draft
            {
                DateIssued = command.DateIssued,
                ProjectName = command.ProjectName,
                Developer = command.Developer,
                Owner = command.Owner,
                CrNo = command.CrNo,
                LsNo = command.LsNo,
                Barangay = command.Barangay,
                Salable = command.Salable,
                IsFm = command.IsFm,
                Remarks = command.Remarks ?? string.Empty,
                MunicipalityId = command.MunicipalityId,
                ProjectTypeId = command.ProjectTypeId,
                
                HasCoc = command.CocDate.HasValue,
                HasDod = command.DodDate.HasValue,
                CocDate = command.CocDate ?? null,
                DodDate = command.DodDate ?? null,
                DateOfCompletion = command.DateOfCompletion,
                ExtensionOfTime = command.ExtensionOfTime ?? null,

                AddedById = userId,
                CreatedAt = DateTimeOffset.UtcNow,
            };

            context.Drafts.Add(draft);
            await context.SaveChangesAsync(ct);

            return AppResult<Response>.Success(new Response(draft.Id, "Draft created successfully."));
        }
    }
}