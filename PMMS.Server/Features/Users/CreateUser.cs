using FluentValidation;
using Mapster;
using MediatR;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PMMS.Server.Common.Constants;
using PMMS.Server.Common.Endpoints;
using PMMS.Server.Common.Result;
using PMMS.Server.Domain.Entities;
using PMMS.Server.Infrastructure.Persistence;

namespace PMMS.Server.Features.Users;

public sealed class CreateUser : IEndpoint {
    public record Command(
        string UserName,
        string Email,
        string Password,
        string Role,
        IReadOnlyList<int>? AssignedProvinceIds
    ) : IRequest<AppResult<Response>>;

    public record Response(
        string Id,
        string Email,
        string UserName,
        string Role,
        string Message
    );

    public class Validator : AbstractValidator<Command>
    {
        public Validator()
        {
            RuleFor(x => x.Email)
                .NotEmpty()
                .EmailAddress()
                .Must(email => email.EndsWith("@dhsud.hredrd.rv", StringComparison.OrdinalIgnoreCase))
                .WithMessage("Only official DHSUD-HREDRD emails are permitted.");

            RuleFor(x => x.Password)
                .NotEmpty()
                .MinimumLength(8);

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
        app.MapPost("users", async (
            [FromBody] Command command,
            ISender sender,
            CancellationToken ct) =>
        {
            var createUserCommand = command.Adapt<Command>();

            var result = await sender.Send(command, ct);
            
            return result.IsSuccess
                ? TypedResults.Created($"/users/{result.Value?.Id}", result)
                : result.ToProblem();
        })
        .WithName("CreateUser")
        .WithTags("Users")
        .WithSummary("Create User")
        .RequireAuthorization("AdminOnly")
        .Produces(StatusCodes.Status201Created)
        .ProducesValidationProblem()
        .ProducesProblem(StatusCodes.Status404NotFound)
        .ProducesProblem(StatusCodes.Status400BadRequest)
        .ProducesProblem(StatusCodes.Status409Conflict)
        .ProducesProblem(StatusCodes.Status401Unauthorized)
        .ProducesProblem(StatusCodes.Status403Forbidden)
        .ProducesProblem(StatusCodes.Status422UnprocessableEntity);
    }


    public class Handler(
        UserManager<ApplicationUser> userManager,
        RoleManager<IdentityRole> roleManager,
        PmmsDbContext context) : IRequestHandler<Command, AppResult<Response>>
    {
        public async Task<AppResult<Response>> Handle(Command command, CancellationToken ct)
        {
            // Normalize role to system constants
            var normalizedRole = NormalizeRole(command.Role);

            // Conflict check
            var existing = await userManager.FindByEmailAsync(command.Email);
            if (existing is not null)
            {
                return AppResult<Response>.Failure("User alrady existed.", ErrorType.Conflict);   
            }

            // Ensure role exists in Identity store
            var roleExists = await roleManager.RoleExistsAsync(normalizedRole);
            if (!roleExists)
            {
                return AppResult<Response>.Failure("Role doesnt exist.", ErrorType.NotFound);   
            }

            // Business rule: permanent users must have at least one assignment
            var provinceIds = (command.AssignedProvinceIds ?? [])
                .Distinct()
                .ToList();

            // Check if Temporary user and prevent Assignment if true
            if (command.Role.Equals("Temporary", StringComparison.OrdinalIgnoreCase)
                && command.AssignedProvinceIds?.Any() == true)
            {
                return AppResult<Response>.Failure("Temporary users cannot have a province assignment.");   
            }

            if (normalizedRole.Equals(UserRoles.Permanent, StringComparison.OrdinalIgnoreCase) && provinceIds.Count == 0)
            {
                return AppResult<Response>.Failure("Permanent users must have at least one province assignment.");   
            }

            // Province ids must exist
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

            var user = new ApplicationUser
            {
                UserName = command.UserName,
                Email = command.Email,
                EmailConfirmed = true
            };

            var createResult = await userManager.CreateAsync(user, command.Password);
            if (!createResult.Succeeded)
            {
                // Convert Identity creation failures into validation-style error
                var errorMessage = string.Join("; ", createResult.Errors.Select(e => e.Description));
                return AppResult<Response>.Failure(
                    errorMessage,
                    ErrorType.Validation
                );
            }

            var roleResult = await userManager.AddToRoleAsync(user, normalizedRole);
            if (!roleResult.Succeeded)
            {
                var errorMessage = string.Join("; ", roleResult.Errors.Select(e => e.Description));
                return AppResult<Response>.Failure(
                    errorMessage,
                    ErrorType.Validation
                );
            }

            if (provinceIds.Count != 0)
            {
                var assignments = provinceIds.Select(provinceId => new Assignment
                {
                    UserId = user.Id,
                    ProvinceId = provinceId
                });

                context.Assignments.AddRange(assignments);
                await context.SaveChangesAsync(ct);
            }

            var res = new Response(
                Id: user.Id,
                Email: user.Email ?? string.Empty,
                UserName: user.UserName,
                Role: normalizedRole,
                Message: "User was created successfully."
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