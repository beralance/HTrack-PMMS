using Mapster;
using MediatR;
using Microsoft.EntityFrameworkCore;
using PMMS.Server.Common.Endpoints;
using PMMS.Server.Common.Result;
using PMMS.Server.Infrastructure.Persistence;

namespace PMMS.Server.Features.ProjectTypes;

public sealed class GetProjectTypes : IEndpoint
{
    public record Query() : IRequest<AppResult<Response>>;
    
    public record Response(
        IReadOnlyList<Dto> ProjectTypes,
        string Message
    );

    public record Dto(
        int Id,
        string Type,
        string Meaning
    );

    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapGet("/project-types", async(ISender sender, CancellationToken ct) =>
        {
            var result = await sender.Send(new Query(), ct);

            return result.IsSuccess
                ?  TypedResults.Ok(result)
                : result.ToProblem();
        })
        .WithName("GetProjectTypes")
        .WithTags("ProjectTypes")
        .WithSummary("Get ProjectTypes")
        .RequireAuthorization("Employee")
        .Produces(StatusCodes.Status200OK)
        .ProducesValidationProblem()
        .ProducesProblem(StatusCodes.Status401Unauthorized)
        .ProducesProblem(StatusCodes.Status403Forbidden);
    }
    public class Handler(PmmsDbContext context) : IRequestHandler<Query, AppResult<Response>>
    {
        public async Task<AppResult<Response>> Handle(Query query, CancellationToken ct)
        {
            // 1. Get municipalities
            var projectTypes = await context.ProjectTypes
                .AsNoTracking()
                .ProjectToType<Dto>()
                .ToListAsync(ct);

            var res = new Response(
                ProjectTypes: projectTypes,
                Message: "Projects are fetched successfully."
            );
            
            return AppResult<Response>.Success(res);
        }
    }
}