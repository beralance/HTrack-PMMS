using FluentValidation;
using MediatR;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using PMMS.Server.Common.Constants;
using PMMS.Server.Common.Endpoints;
using PMMS.Server.Common.Result;
using PMMS.Server.Common.Security;
using PMMS.Server.Domain.Entities;

namespace PMMS.Server.Features.Users;

public sealed class DeleteUser : IEndpoint {

    public record Command(string Id) : IRequest<AppResult<Response>>;

    
    public record Response(
        string Id,
        string Message);


    public class Validator : AbstractValidator<Command>
    {
        public Validator()
        {
            RuleFor(x => x.Id).NotEmpty();
        }
    }


    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapDelete("users/{id}", async (string id, ISender sender, CancellationToken ct) =>
        {
            var result = await sender.Send(new Command(id), ct);

            return result.IsSuccess
                ? TypedResults.Ok(result)
                : result.ToProblem();
        })
        .WithName("DeleteUser")
        .WithTags("Users")
        .WithSummary("Delete User")
        .RequireAuthorization("AdminOnly")
        .Produces(StatusCodes.Status200OK)
        .ProducesValidationProblem()
        .ProducesProblem(StatusCodes.Status400BadRequest)
        .ProducesProblem(StatusCodes.Status404NotFound)
        .ProducesProblem(StatusCodes.Status403Forbidden)
        .ProducesProblem(StatusCodes.Status401Unauthorized)
        .ProducesProblem(StatusCodes.Status422UnprocessableEntity);
    }


    public class Handler(
        UserManager<ApplicationUser> userManager,
        IUserContext userContext) : IRequestHandler<Command, AppResult<Response>>
    {
        public async Task<AppResult<Response>> Handle(Command command, CancellationToken ct)
        {
            var user = await userManager.Users
                .FirstOrDefaultAsync(u => u.Id == command.Id, ct);
            
            if (user is null)
            {
                return AppResult<Response>.Failure($"User {command.Id} was not found.", ErrorType.NotFound);
            }

            if (!string.IsNullOrWhiteSpace(userContext.UserId) &&
                user.Id == userContext.UserId)
            {
                return AppResult<Response>.Failure($"Own account cannot be deleted.");
            }

            var roles = await userManager.GetRolesAsync(user);
            if (roles.Any(r => r.Equals(UserRoles.Admin, StringComparison.OrdinalIgnoreCase)))
            {
                var admins = await userManager.GetUsersInRoleAsync(UserRoles.Admin);
                var activeAdmins = admins.Where(a => a.IsDeleted == false).ToList();

                if (activeAdmins.Count <= 1)
                {                    
                    return AppResult<Response>.Failure($"Cannot delete the last active admin account.");
                }
            }

            user.IsDeleted = true;
            user.DeletedAt = DateTime.UtcNow;
            user.DeletedById = userContext.UserId;

            user.LockoutEnabled = true;
            user.LockoutEnd = DateTimeOffset.MaxValue;

            var stampResult = await userManager.UpdateSecurityStampAsync(user);
            if (!stampResult.Succeeded)
            {
                var msg = string.Join("; ", stampResult.Errors.Select(e => e.Description));
                throw new ValidationException(msg);
            }

            var updateResult = await userManager.UpdateAsync(user);
            if (!updateResult.Succeeded)
            {
                var msg = string.Join("; ", updateResult.Errors.Select(e => e.Description));
                throw new ValidationException(msg);
            }

            var res = new Response(
                Id: user.Id,
                Message: "User was deleted successfully."
            );

            return AppResult<Response>.Success(res);
        }
    }
}