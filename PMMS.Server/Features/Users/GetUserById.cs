using FluentValidation;
using Mapster;
using MediatR;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using PMMS.Server.Common.Endpoints;
using PMMS.Server.Common.Result;
using PMMS.Server.Domain.Entities;
using PMMS.Server.Infrastructure.Persistence;

namespace PMMS.Server.Features.Users;


public sealed class GetUserById : IEndpoint {
    public record Query(
        string Id,
        bool IncludeDeleted = false
    ) : IRequest<AppResult<Response>>;

        
    public record Response(
        GetUserByIdDto User,
        string Message
    );


    public record GetUserByIdDto (
        string Id,
        string Email,
        string? UserName,
        bool IsDeleted,
        IReadOnlyList<string> Roles,
        IReadOnlyList<int> AssignedProvinces
    );


    public class GetUserByIdValidator : AbstractValidator<Query>
    {
        public GetUserByIdValidator()
        {
            RuleFor(x => x.Id).NotEmpty();
        }
    }


    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapGet("users/{Id}", async (
            [AsParameters]
            Query query,
            ISender sender,
            CancellationToken ct) =>
        {
            var result = await sender.Send(query, ct);

            return result.IsSuccess
                ? TypedResults.Ok(result)
                : result.ToProblem();
        })
        .WithName("GetUserById")
        .WithTags("Users")
        .WithSummary("Get User by Id")
        .RequireAuthorization("AdminOnly")
        .Produces(StatusCodes.Status200OK)
        .ProducesProblem(StatusCodes.Status401Unauthorized)
        .ProducesProblem(StatusCodes.Status404NotFound);
    }


    public class Handler(
        UserManager<ApplicationUser> userManager,
        PmmsDbContext context) : IRequestHandler<Query, AppResult<Response>>
    {
        public async Task<AppResult<Response>> Handle(Query query, CancellationToken ct)
        {
            var userQuery = userManager.Users.AsNoTracking().Where(u => u.Id == query.Id);

            if (!query.IncludeDeleted)
                userQuery = userQuery.Where(u => u.IsDeleted == false);

            var user = await userQuery.FirstOrDefaultAsync(ct);

             if (user is null)
            {
                return AppResult<Response>.Failure("", ErrorType.NotFound);
            }

            var roles = await userManager.GetRolesAsync(user);

            var assignedProvinces = await context.Assignments
                .AsNoTracking()
                .Where(a => a.UserId == user.Id)
                .Select(a => a.ProvinceId)
                .ToListAsync(ct);

            var userDto = user.Adapt<GetUserByIdDto>();

            var res = new Response(
                User: userDto,
                Message: "User was successfully fetched."
            );

            return AppResult<Response>.Success(res);
        }
    }
}