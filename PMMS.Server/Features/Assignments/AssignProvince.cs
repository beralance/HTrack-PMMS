using FluentValidation;
using MediatR;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using PMMS.Server.Common.Constants;
using PMMS.Server.Common.Endpoints;
using PMMS.Server.Common.Result;
using PMMS.Server.Domain.Entities;
using PMMS.Server.Infrastructure.Persistence;

namespace PMMS.Server.Features.Assignments;


public sealed class AssignProvince : IEndpoint {

    public record Command(
        string UserId, 
        List<int> ProvinceIds
    ) : IRequest<AppResult<Response>>;


    public record Response (
        string Messsage
    );


    public class Validator : AbstractValidator<Command>
    {
        public Validator()
        {
            RuleFor(x => x.UserId)
                .NotEmpty()
                .WithMessage("User selection is required.");
            
            RuleFor(x => x.ProvinceIds)
                .NotEmpty()
                .WithMessage("At least one province must be assigned.")
                .Must(p => p.Distinct().Count() == p.Count).WithMessage("Duplicate provinces are not allowed.");
        }
    }


    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapPost("assignments", async ( Command command, ISender sender, CancellationToken ct) =>
        {
            var result = await sender.Send(command, ct);

            return result.IsSuccess
                ?  TypedResults.Ok(result)
                : result.ToProblem();
        })
        .WithName("AssignProvince")
        .WithTags("Assignments")
        .WithSummary("Assign Province")
        .RequireAuthorization("AdminOnly")
        .ProducesValidationProblem()
        .Produces(StatusCodes.Status200OK)
        .ProducesProblem(StatusCodes.Status404NotFound)
        .ProducesProblem(StatusCodes.Status400BadRequest);
    }


    public class Handler(PmmsDbContext context, UserManager<ApplicationUser> userManager) 
        : IRequestHandler<Command, AppResult<Response>>
    {
        public async Task<AppResult<Response>> Handle(Command command, CancellationToken ct)
        {
            // 1. Verify if user exists and is not soft deleted
            var user = await userManager.FindByIdAsync(command.UserId);
            if (user == null || user.IsDeleted == true)
            {
                return AppResult<Response>.Failure("User does not exist.", ErrorType.NotFound);
            }

            // 2. Check if given user has a permanent role. Only permanent user can be assigned to a province
            var roles = await userManager.GetRolesAsync(user);
            if (!roles.Contains(UserRoles.Permanent))
            {
                return AppResult<Response>.Failure(
                    $"User {user.UserName} is not a Permanent employee. Only permanent staff require regional assignments.");
            }

            // 3. Verify all ProvinceIds actually exist in the database
            var validProvinceCount = await context.Provinces
                .CountAsync(p => command.ProvinceIds.Contains(p.Id), ct);

            if (validProvinceCount != command.ProvinceIds.Count)
            {
                return AppResult<Response>.Failure("One or more provided Province IDs are invalid.");
            }

            // 4. Clear existing assignment and Add new
            var existingAssignments = await context.Assignments
                .Where(a => a.UserId == command.UserId)
                .ToListAsync(ct);

            if (existingAssignments.Count != 0)
                context.Assignments.RemoveRange(existingAssignments);

            var newAssignments = command.ProvinceIds.Select(provinceId => new PMMS.Server.Domain.Entities.Assignment
            {
                UserId = command.UserId,
                ProvinceId = provinceId,
                CreatedAt = DateTimeOffset.UtcNow
            });

            await context.Assignments.AddRangeAsync(newAssignments, ct);
            await context.SaveChangesAsync(ct);

            var res = new Response("Province assigned successfully!");
         
            return AppResult<Response>.Success(res);
        }
    }
}