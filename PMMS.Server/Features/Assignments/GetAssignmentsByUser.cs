using Mapster;
using MediatR;
using Microsoft.EntityFrameworkCore;
using PMMS.Server.Common.Endpoints;
using PMMS.Server.Common.Result;
using PMMS.Server.Infrastructure.Persistence;

namespace PMMS.Server.Features.Assignments;


public sealed class GetAssignmentByUser : IEndpoint {
    public record Query(string UserId) : IRequest<AppResult<Response>>;


    public record Response(
        string UserId,
        string UserName,
        string Email,
        string Message,
        IReadOnlyList<Dto> Assignments
    );


    public record Dto(
        int ProvinceId,
        string ProvinceName,
        DateTimeOffset AssignedAt
    );


    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapGet("users/{userId}/assignments", async (string userId, ISender sender, CancellationToken ct) =>
            {
                var result = await sender.Send(new Query(userId), ct);

                return result.IsSuccess 
                    ? TypedResults.Ok(result)
                    : result.ToProblem();
            })
            .WithName("GetAssignmentsByUser")
            .WithTags("Assignments")
            .WithSummary("Get Assignments by User")
            .RequireAuthorization("AdminOnly") 
            .Produces(StatusCodes.Status200OK)
            .ProducesProblem(StatusCodes.Status404NotFound);
    }


    public class Handler(PmmsDbContext context) 
        : IRequestHandler<Query, AppResult<Response>>
    {
        public async Task<AppResult<Response>> Handle(Query query, CancellationToken ct)
        {
            // 1. Fetch User based on the given Id and get its info
            var user = await context.Users
                .AsNoTracking()
                .Select(u => new { u.Id, u.UserName, u.Email, u.IsDeleted })
                .FirstOrDefaultAsync(u => u.Id == query.UserId, ct);

            // 2. Verify if there is a User or if its soft deleted
            if (user == null || user.IsDeleted == true)
            {
                return AppResult<Response>.Failure($"User {query.UserId} was not found.", ErrorType.NotFound);
            }

            // 3. Map to DTO
            var assignments = await context.Assignments
                .AsNoTracking()
                .Where(a => a.UserId == query.UserId)
                .Include(a => a.Province)
                .OrderBy(a => a.Province.ProvinceName)
                .ProjectToType<Dto>()
                .ToListAsync(ct);

            var res = new Response(
                UserId: user.Id,
                UserName: user.UserName ?? "Unknown",
                Email: user.Email ?? string.Empty,
                Message: "User Assignments are fetched successfully.",
                Assignments: assignments
            );

            return AppResult<Response>.Success(res);
        }
    }
}