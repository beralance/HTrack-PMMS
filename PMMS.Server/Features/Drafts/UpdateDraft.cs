using FluentValidation;
using Mapster;
using MediatR;
using Microsoft.EntityFrameworkCore;
using PMMS.Server.Common.Endpoints;
using PMMS.Server.Common.Result;
using PMMS.Server.Common.Security;
using PMMS.Server.Domain.Entities;
using PMMS.Server.Infrastructure.Persistence;

namespace PMMS.Server.Features.Drafts;


public sealed class UpdateDraft : IEndpoint {
    
    public record Command(
        Guid Id,
        DateOnly DateIssued,
        string ProjectName,
        string Developer,
        string Owner,
        string CrNo,
        string LsNo,
        string Barangay,
        string Salable,
        bool IsFm,
        int MunicipalityId,
        int ProjectTypeId,
        string? Remarks,

        DateOnly? CocDate,
        DateOnly? DodDate,
        DateOnly DateOfCompletion,
        DateOnly? ExtensionOfTime,
        DateOnly? PerformanceBond,
        DateOnly? SemestralReport,

        string ActorId
    ) : IRequest<AppResult<Response>>;

    public record Response(
        Guid Id,
        string Message);

    public class Validator : AbstractValidator<Command>
    {
        public Validator()
        {
            RuleFor(x => x.Id).NotEmpty();
            RuleFor(x => x.DateIssued).NotEmpty();
            RuleFor(x => x.ProjectName).NotEmpty().MaximumLength(100);
            RuleFor(x => x.Developer).NotEmpty().MaximumLength(100);
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
        app.MapPut("drafts/{id:Guid}", async(
            Guid id, 
            Command command, 
            ISender sender, 
            CancellationToken ct) =>
        {
            if (id != command.Id)
            {
                return TypedResults.BadRequest("Invalid Id: Mismatch.");
            }

            var result = await sender.Send(command, ct);

            return result.IsSuccess
                ? TypedResults.Ok(result)
                : result.ToProblem();
        })
        .WithName("UpdateDraft")
        .WithTags("Drafts")
        .WithSummary("Update draft")
        .RequireAuthorization("TemporaryOnly")
        .Produces(StatusCodes.Status200OK)
        .ProducesValidationProblem()
        .ProducesProblem(StatusCodes.Status404NotFound)
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
                return AppResult<Response>.Failure("Unauthorized.", ErrorType.Unauthorized);

            // 1. Fetch only the user's own unpublished draft
            var draft = await context.Drafts
                .FirstOrDefaultAsync(d => d.Id == command.Id && 
                                        d.IsPublished == false && 
                                        d.AddedById == userId, ct);
                
            if (draft is null)
            {
                return AppResult<Response>.Failure($"Draft {command.Id} was not found or access denied.", ErrorType.NotFound);
            }

            // 2. Check for Name Collision (excluding self)
            var nameNormalized = command.ProjectName.Trim().ToLower();
            var exists = await context.Drafts
                .AnyAsync(d => d.ProjectName.ToLower() == nameNormalized && 
                            d.Id != command.Id && 
                            d.AddedById == userId, ct);

            if (exists)
            {
                return AppResult<Response>.Failure($"A draft named '{command.ProjectName}' already exists.", ErrorType.Conflict);
            }

            // 3. Update data (Manual mapping is safer and more performant than per-request Mapster config)
            draft.DateIssued = command.DateIssued;
            draft.ProjectName = command.ProjectName.Trim();
            draft.Developer = command.Developer.Trim();
            draft.Owner = command.Owner.Trim();
            draft.CrNo = command.CrNo.Trim();
            draft.LsNo = command.LsNo.Trim();
            draft.Barangay = command.Barangay.Trim();
            draft.Salable = command.Salable.Trim();
            draft.IsFm = command.IsFm;
            draft.Remarks = command.Remarks?.Trim();
            draft.MunicipalityId = command.MunicipalityId;
            draft.ProjectTypeId = command.ProjectTypeId;
            
            draft.CocDate = command.CocDate;
            draft.HasCoc = command.CocDate.HasValue;
            draft.DodDate = command.DodDate;
            draft.HasDod = command.DodDate.HasValue;
            draft.DateOfCompletion = command.DateOfCompletion;
            draft.ExtensionOfTime = command.ExtensionOfTime;
            draft.PerformanceBond = command.PerformanceBond;
            draft.SemestralReport = command.SemestralReport;
            
            draft.UpdatedById = userId;
            draft.UpdatedAt = DateTimeOffset.UtcNow;

            await context.SaveChangesAsync(ct);

            return AppResult<Response>.Success(new Response(draft.Id, "Draft updated successfully."));
        }
    }
}