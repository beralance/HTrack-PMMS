using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;
using PMMS.Server.Common.Endpoints;
using PMMS.Server.Common.Security;
using PMMS.Server.Domain.Entities;
using PMMS.Server.Common.Result;
using PMMS.Server.Infrastructure.Persistence;
using PMMS.Server.Domain.Enums;

namespace PMMS.Server.Features.Projects;

public sealed class CreateProject : IEndpoint {
    
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
        int ProjectTypeId
    ) : IRequest<AppResult<Response>>;

    public record Response (
        Guid Id,
        DateOnly DateIssued,
        string Message
    );

    public class Validator : AbstractValidator<Command>
    {
        public Validator()
        {
            RuleFor(x => x.DateIssued)
                .NotEmpty()
                .LessThanOrEqualTo(x => DateOnly.FromDateTime(DateTime.Today))
                .WithMessage("Date Issued cannot be in the future.");
            RuleFor(x => x.ProjectName).NotEmpty().MaximumLength(200);
            RuleFor(x => x.Developer).NotEmpty().MaximumLength(200);
            RuleFor(x => x.Owner).NotEmpty().WithMessage("Owner is required.").MaximumLength(200);
            RuleFor(x => x.CrNo).NotEmpty().WithMessage("Registration Number (CR No) is required.");
            RuleFor(x => x.LsNo).NotEmpty().WithMessage("License to Sell (LS No) is required.");
            RuleFor(x => x.Barangay).NotEmpty();
            RuleFor(x => x.Salable).NotEmpty();
            RuleFor(x => x.IsFm).NotNull();
            RuleFor(x => x.MunicipalityId).GreaterThan(0);
            RuleFor(x => x.ProjectTypeId).GreaterThan(0);
        }
    }

    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapPost("projects", async (Command command, ISender sender, CancellationToken ct) =>
            {
                var result = await sender.Send(command, ct);
                
                return result.IsSuccess
                    ? TypedResults.Created($"/projects/{result.Value?.Id}", result)
                    : result.ToProblem();
            })
            .WithName("CreateProject")
            .WithTags("Projects")
            .WithSummary("Create project")
            .RequireAuthorization("PermanentOnly")
            .Produces(StatusCodes.Status201Created)
            .ProducesValidationProblem()
            .ProducesProblem(StatusCodes.Status403Forbidden)
            .ProducesProblem(StatusCodes.Status400BadRequest)
            .ProducesProblem(StatusCodes.Status409Conflict)
            .ProducesProblem(StatusCodes.Status422UnprocessableEntity);
    }

    public class Handler(
        PmmsDbContext context,
        IUserContext userContext) : IRequestHandler<Command, AppResult<Response>>
    {
        public async Task<AppResult<Response>> Handle(Command command, CancellationToken ct)
        {
            // 1. Check if Project name already exists (Case-Insensitive Normalization)
            var normalizedName = command.ProjectName.Trim().ToLower();
            var isDuplicate = await context.Projects
                .AnyAsync(p => p.ProjectName.ToLower() == normalizedName && !p.IsDeleted, ct);

            if (isDuplicate)
            {
                return AppResult<Response>.Failure($"Project '{command.ProjectName}' already exists.", ErrorType.Conflict);
            }

            // 2. Get the province of the chosen municipality
            var municipality = await context.Municipalities
                .AsNoTracking()
                .FirstOrDefaultAsync(m => m.Id == command.MunicipalityId, ct);

            if (municipality is null)
            {
                return AppResult<Response>.Failure("The specified municipality does not exist.", ErrorType.Validation);
            }

            // 3. Verify if User can add the Project based on his Assigned Province boundaries
            var userProvinces = userContext.AssignedProvinceIds;
            
            if (!userProvinces.Contains(municipality.ProvinceId))
            {
                return AppResult<Response>.Failure(
                    $"Access Denied: You are not assigned to the province of {municipality.MunicipalityName}.",
                    ErrorType.Forbidden);
            }

            // 4. Map and Persist data rows
            var project = new Project
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
                
                AddedById = userContext.UserId ?? string.Empty,
                IsPublished = true,
                SetupStatus = ProjectSetupStatuses.NotTracked,
                IsDeleted = false,
                CreatedAt = DateTimeOffset.UtcNow
            };

            context.Projects.Add(project);
            await context.SaveChangesAsync(ct);

            var res = new Response(
                Id: project.Id,
                DateIssued: project.DateIssued,
                Message: "Project was created successfully."
            );
            
            return AppResult<Response>.Success(res);
        }
    }
}