using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;
using PMMS.Server.Common.Endpoints;
using PMMS.Server.Common.Result;
using PMMS.Server.Common.Security;
using PMMS.Server.Domain.Entities;
using PMMS.Server.Infrastructure.Persistence;

namespace PMMS.Server.Features.Projects;

public sealed class UpdateProject : IEndpoint {
   
    public record Command(
        Guid Id,
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
        int ProjectTypeId
    ) : IRequest<AppResult<Response>>;

    public record Response(
        Guid Id,
        string Message
    );

    public class Validator : AbstractValidator<Command>
    {
        public Validator()
        {
            RuleFor(x => x.ProjectName).NotEmpty().MaximumLength(200);
            RuleFor(x => x.Developer).NotEmpty().MaximumLength(200);
            RuleFor(x => x.CrNo).NotEmpty().MaximumLength(50);
            RuleFor(x => x.LsNo).NotEmpty().MaximumLength(50);
            RuleFor(x => x.Barangay).NotEmpty().MaximumLength(200);
            RuleFor(x => x.Salable).NotEmpty().MaximumLength(50);
            RuleFor(x => x.Remarks).MaximumLength(500);
            RuleFor(x => x.ProjectTypeId).GreaterThan(0);
            RuleFor(x => x.MunicipalityId).GreaterThan(0);
        }
    }

    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapPut("projects/{id:guid}", async (Guid id, Command command, ISender sender, CancellationToken ct) =>
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
            .WithName("UpdateProject")
            .WithTags("Projects")
            .WithSummary("Update project")
            .RequireAuthorization("PermanentOnly")
            .ProducesValidationProblem()
            .Produces(StatusCodes.Status200OK)
            .ProducesProblem(StatusCodes.Status404NotFound)
            .ProducesProblem(StatusCodes.Status403Forbidden)
            .ProducesProblem(StatusCodes.Status400BadRequest)
            .ProducesProblem(StatusCodes.Status422UnprocessableEntity);
    }

    public class Handler(
        PmmsDbContext context,
        IUserContext userContext) : IRequestHandler<Command, AppResult<Response>>
    {
        public async Task<AppResult<Response>> Handle(Command command, CancellationToken ct)
        {
            var assignedProvinces = userContext.AssignedProvinceIds;

            var project = await context.Projects
                .Include(p => p.Municipality)
                .FirstOrDefaultAsync(p => p.Id == command.Id, ct);

            if (project == null || project.IsDeleted)
            {
                return AppResult<Response>.Failure($"Project {command.Id} was not found.", ErrorType.NotFound);
            }

            if (!assignedProvinces.Contains(project.Municipality.ProvinceId))
            {
                return AppResult<Response>.Failure(
                    "You do not have permission to update projects in this region.", 
                    ErrorType.Forbidden);
            }

            if (project.MunicipalityId != command.MunicipalityId)
            {
                var targetMunicipality = await context.Municipalities
                    .AsNoTracking()
                    .FirstOrDefaultAsync(m => m.Id == command.MunicipalityId, ct);
                
                if (targetMunicipality is null)
                {
                    return AppResult<Response>.Failure("The selected Municipality does not exist.", ErrorType.Validation);
                }

                if (!assignedProvinces.Contains(targetMunicipality.ProvinceId))
                {
                    return AppResult<Response>.Failure(
                        $"Access Denied: You are not authorized to move projects into {targetMunicipality.MunicipalityName}.", 
                        ErrorType.Forbidden);
                }
            }

            var normalizedName = command.ProjectName.Trim().ToLower();
            var isDuplicateName = await context.Projects
                .AnyAsync(p => p.Id != command.Id && p.ProjectName.ToLower() == normalizedName && !p.IsDeleted, ct);

            if (isDuplicateName)
            {
                return AppResult<Response>.Failure($"Another active project named '{command.ProjectName}' already exists.", ErrorType.Conflict);
            }

            project.ProjectName = command.ProjectName;
            project.Developer = command.Developer;
            project.Owner = command.Owner;
            project.CrNo = command.CrNo;
            project.LsNo = command.LsNo;
            project.Barangay = command.Barangay;
            project.Salable = command.Salable;
            project.IsFm = command.IsFm;
            project.Remarks = command.Remarks ?? string.Empty;
            project.MunicipalityId = command.MunicipalityId;
            project.ProjectTypeId = command.ProjectTypeId;

            project.IsModified = true;
            project.ModifiedById = userContext.UserId ?? string.Empty;
            project.UpdatedAt = DateTimeOffset.UtcNow;

            await context.SaveChangesAsync(ct);

            var res = new Response(
                Id: project.Id,
                Message: "Project updated successfully."
            );

            return AppResult<Response>.Success(res);
        }
    }
}