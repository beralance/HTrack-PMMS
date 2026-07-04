using FluentValidation;
using MediatR;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using PMMS.Server.Common.Endpoints;
using PMMS.Server.Domain.Entities;
using PMMS.Server.Common.Result;
using PMMS.Server.Common.Constants;
using PMMS.Server.Infrastructure.Persistence;

namespace PMMS.Server.Features.Users;


public sealed class GetUsers : IEndpoint {

    public record Query(
        int Page = 1,
        int PageSize = 20,
        string? Search = null,
        string? Role = null,
        bool IncludeDeleted = false
    ) : IRequest<AppResult<Response>>;

        
    public record Response(
        IReadOnlyList<Dto> Items,
        int Page,
        int PageSize,
        int TotalCount,
        string Message
    );

    public record Dto
    (
        string Id,
        string Email,
        string UserName,
        bool IsDeleted,
        DateTimeOffset CreatedAt,
        IReadOnlyList<string> Roles,
        IReadOnlyList<int> AssignedProvinces
    );


    public class Validator : AbstractValidator<Query>
    {
        public Validator()
        {
            RuleFor(x => x.Page).GreaterThan(0);
            RuleFor(x => x.PageSize).GreaterThan(0).LessThanOrEqualTo(100);

            RuleFor(x => x.Role)
                .Must(role =>
                    role is null ||
                    role.Equals(UserRoles.Admin, StringComparison.OrdinalIgnoreCase) ||
                    role.Equals(UserRoles.Permanent, StringComparison.OrdinalIgnoreCase) ||
                    role.Equals(UserRoles.Temporary, StringComparison.OrdinalIgnoreCase))
                .WithMessage("Invalid role filter.");
        }
    }


    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapGet("users", async ([AsParameters] Query query, ISender sender, CancellationToken ct) =>
        {
            var result = await sender.Send(query, ct);
            
            return result.IsSuccess
                ? TypedResults.Ok(result)
                : result.ToProblem();
        })
        .WithName("GetUsers")
        .WithTags("Users")
        .WithSummary("Get Users")
        .RequireAuthorization("AdminOnly")
        .Produces(StatusCodes.Status200OK)
        .ProducesValidationProblem()
        .ProducesProblem(StatusCodes.Status401Unauthorized)
        .ProducesProblem(StatusCodes.Status403Forbidden);
    }


    public class Handler(
        UserManager<ApplicationUser> userManager,
        PmmsDbContext context) : IRequestHandler<Query, AppResult<Response>>
    {
        public async Task<AppResult<Response>> Handle(Query query, CancellationToken ct)
        {
            var usersQuery = userManager.Users.AsNoTracking();

            if (!query.IncludeDeleted)
                usersQuery = usersQuery.Where(u => u.IsDeleted == false);

            if (!string.IsNullOrWhiteSpace(query.Search))
            {
                var search = query.Search.Trim().ToLower();
                usersQuery = usersQuery.Where(u =>
                    u.UserName!.ToLower().Contains(search) ||
                    u.Email!.ToLower().Contains(search)
                );
            }

            if (!string.IsNullOrWhiteSpace(query.Role))
            {
                usersQuery = from user in usersQuery
                            join userRole in context.UserRoles on user.Id equals userRole.UserId
                            join role in context.Roles on userRole.RoleId equals role.Id
                            where role.Name == query.Role.ToUpper()
                            select user;
            }

            var totalCount = await usersQuery.CountAsync(ct);

            var users = await usersQuery
                .OrderBy(u => u.UserName)
                .Skip((query.Page - 1) * query.PageSize)
                .Take(query.PageSize)
                .ToListAsync(ct);

            var userIds = users.Select(u => u.Id).ToList();

            var assignments = await context.Assignments
                .AsNoTracking()
                .Where(a => a.UserId != null && userIds.Contains(a.UserId))
                .ToListAsync(ct);

            var provinceLookup = assignments
                .GroupBy(x => x.UserId!)
                .ToDictionary(g => g.Key, g => g.Select(x => x.ProvinceId).ToList());

            var userRoles = await (from ur in context.UserRoles
                                join r in context.Roles on ur.RoleId equals r.Id
                                where userIds.Contains(ur.UserId)
                                select new { ur.UserId, r.Name })
                                .ToListAsync(ct);

            var roleLookup = userRoles
                .GroupBy(x => x.UserId)
                .ToDictionary(g => g.Key, g => g.Select(x => x.Name!).ToList());

            var items = users.Select(user => new Dto(
                Id: user.Id,
                Email: user.Email ?? string.Empty,
                UserName: user.UserName!,
                IsDeleted: user.IsDeleted ?? default,
                CreatedAt: user.CreatedAt,
                Roles: roleLookup.GetValueOrDefault(user.Id) ?? [],
                AssignedProvinces: provinceLookup.GetValueOrDefault(user.Id) ?? []
            )).ToList();
            
            var res = new Response(
                Items: items,
                Page: query.Page,
                PageSize: query.PageSize,
                TotalCount: totalCount,
                Message: "Users are successfully fetched."
            );

            return AppResult<Response>.Success(res);
        }
    }
}