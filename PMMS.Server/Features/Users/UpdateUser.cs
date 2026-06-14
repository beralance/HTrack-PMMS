using FluentValidation;
using Mapster;
using MediatR;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using PMMS.Server.Common.Constants;
using PMMS.Server.Common.Endpoints;
using PMMS.Server.Common.Result;
using PMMS.Server.Common.Security;
using PMMS.Server.Domain.Entities;
using PMMS.Server.Infrastructure.Persistence;

namespace PMMS.Server.Features.Users;

public sealed class UpdateUser : IEndpoint {

    public record Command(
        string Id,
        string Email,
        string Role,
        IReadOnlyList<int>? AssignedProvinceIds
    ) : IRequest<AppResult<Response>>;

    
    public record Response(
        Dto User,
        string Message
    );


    public record Dto(
        string Id,
        string Email,
        string Role,
        IReadOnlyList<int> AssignedProvinces
    );


    public class Validator : AbstractValidator<Command>
    {
        public Validator()
        {
            RuleFor(x => x.Id).NotEmpty();

            RuleFor(x => x.Email)
                .NotEmpty()
                .EmailAddress()
                .Must(email => email.EndsWith("@dhsud.hredrd.rv", StringComparison.OrdinalIgnoreCase))
                .WithMessage("Only official DHSUD-HREDRD emails are permitted.");

            RuleFor(x => x.Role)
                .NotEmpty()
                .Must(role =>
                    role.Equals(UserRoles.Admin, StringComparison.OrdinalIgnoreCase) ||
                    role.Equals(UserRoles.Permanent, StringComparison.OrdinalIgnoreCase) ||
                    role.Equals(UserRoles.Temporary, StringComparison.OrdinalIgnoreCase))
                .WithMessage("Invalid role.");

            RuleFor(x => x.AssignedProvinceIds)
                .Must(list => list is null || list.Distinct().Count() == list.Count)
                .WithMessage("Assigned provinces must not contain duplicates.");
        }
    }


    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapPut("users/{id}", async (
                string id,
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
            .WithName("UpdateUser")
            .WithTags("Users")
            .WithSummary("Update User")
            .RequireAuthorization("AdminOnly")
            .Produces(StatusCodes.Status200OK)
            .ProducesValidationProblem()
            .ProducesProblem(StatusCodes.Status401Unauthorized)
            .ProducesProblem(StatusCodes.Status403Forbidden)
            .ProducesProblem(StatusCodes.Status404NotFound)
            .ProducesProblem(StatusCodes.Status400BadRequest)
            .ProducesProblem(StatusCodes.Status409Conflict)
            .ProducesProblem(StatusCodes.Status422UnprocessableEntity);
    }


    public class Handler(
        UserManager<ApplicationUser> userManager,
        RoleManager<IdentityRole> roleManager,
        PmmsDbContext context,
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

            var userRole = await userManager.GetRolesAsync(user);

            if (userRole.Contains(UserRoles.Temporary) &&
                command.AssignedProvinceIds != null &&
                command.AssignedProvinceIds.Any())
            {
                return AppResult<Response>.Failure($"Temporary users should not have an assigned province.");
            }

            var normalizedRole = NormalizeRole(command.Role);

            var roleExists = await roleManager.RoleExistsAsync(normalizedRole);
            
            if (!roleExists)
            {
                return AppResult<Response>.Failure($"Role {command.Role} does not exist.", ErrorType.NotFound);
            }

            // prevent email conflict with other users
            var existingByEmail = await userManager.FindByEmailAsync(command.Email);
            if (existingByEmail is not null && existingByEmail.Id != user.Id)
            {
                return AppResult<Response>.Failure($"User emeail {command.Email} already existed.", ErrorType.Conflict);
            }

            var provinceIds = (command.AssignedProvinceIds ?? [])
                .Distinct()
                .ToList();

            // Domain rule: permanent users must have assignment
            if (normalizedRole.Equals(UserRoles.Permanent, StringComparison.OrdinalIgnoreCase) && provinceIds.Count == 0)
            {
                return AppResult<Response>.Failure("Permanent users must have at least one province assignment.");
            }

            // validate province ids
            if (provinceIds.Count != 0)
            {
                var validProvinceIds = await context.Provinces
                    .AsNoTracking()
                    .Where(p => provinceIds.Contains(p.Id))
                    .Select(p => p.Id)
                    .ToListAsync(ct);

                var missing = provinceIds.Except(validProvinceIds).ToList();
                if (missing.Count != 0)
                {
                    return AppResult<Response>.Failure(string.Join(",", missing), ErrorType.NotFound);
                }
            }

            // Optional guardrail: prevent self-demotion/removal from admin role
            if (!string.IsNullOrWhiteSpace(userContext.UserId) &&
                user.Id == userContext.UserId &&
                !normalizedRole.Equals(UserRoles.Admin, StringComparison.OrdinalIgnoreCase))
            {
                return AppResult<Response>.Failure("Own account cannot be removed.");
            }

            // update email/username
            user.Email = command.Email;
            user.UserName = command.Email;

            var updateResult = await userManager.UpdateAsync(user);
            if (!updateResult.Succeeded)
            {
                var errorMessage = string.Join("; ", updateResult.Errors.Select(e => e.Description));
                throw new ValidationException(errorMessage);
            }

            // replace roles (single-role model)
            var currentRoles = await userManager.GetRolesAsync(user);
            if (currentRoles.Any())
            {
                var removeRolesResult = await userManager.RemoveFromRolesAsync(user, currentRoles);
                if (!removeRolesResult.Succeeded)
                {
                    var errorMessage = string.Join("; ", removeRolesResult.Errors.Select(e => e.Description));
                    throw new ValidationException(errorMessage);
                }
            }

            var addRoleResult = await userManager.AddToRoleAsync(user, normalizedRole);
            if (!addRoleResult.Succeeded)
            {
                var errorMessage = string.Join("; ", addRoleResult.Errors.Select(e => e.Description));
                throw new ValidationException(errorMessage);
            }

            // replace assignments
            var existingAssignments = await context.Assignments
                .Where(a => a.UserId == user.Id)
                .ToListAsync(ct);

            if (existingAssignments.Count > 0)
                context.Assignments.RemoveRange(existingAssignments);

            if (provinceIds.Count != 0)
            {
                var newAssignments = provinceIds.Select(pid => new Assignment
                {
                    UserId = user.Id,
                    ProvinceId = pid
                });

                context.Assignments.AddRange(newAssignments);
            }

            var userDto = user.Adapt<Dto>();

            await context.SaveChangesAsync(ct);

            var res = new Response(
                User: userDto,
                Message: "User updated successfully."
            );

            return AppResult<Response>.Success(res);
        }

        private static string NormalizeRole(string role)
        {
            if (role.Equals(UserRoles.Admin, StringComparison.OrdinalIgnoreCase)) return UserRoles.Admin;
            if (role.Equals(UserRoles.Permanent, StringComparison.OrdinalIgnoreCase)) return UserRoles.Permanent;
            if (role.Equals(UserRoles.Temporary, StringComparison.OrdinalIgnoreCase)) return UserRoles.Temporary;
            return role;
        }
    }
}