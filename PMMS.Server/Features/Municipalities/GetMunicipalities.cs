using FluentValidation.Validators;
using Mapster;
using MediatR;
using Microsoft.EntityFrameworkCore;
using PMMS.Server.Common.Endpoints;
using PMMS.Server.Common.Result;
using PMMS.Server.Common.Security;
using PMMS.Server.Domain.Entities;
using PMMS.Server.Infrastructure.Persistence;
using System.Security.Claims;

namespace PMMS.Server.Features.Municipalities;

public sealed class GetMunicipalities : IEndpoint
{
    public record Query(bool ByAssignmentOnly = false) : IRequest<AppResult<Response>>;

    public record Response(
        IReadOnlyList<Dto> Municipalities,
        string Message
    );

    public record Dto (
        int Id,
        string MunicipalityName,
        int ProvinceId,
        Province Province
    );

    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapGet("municipalities", async(bool byAssignmentOnly, ISender sender, CancellationToken ct) =>
        {
            var result = await sender.Send(new Query(byAssignmentOnly), ct);

            return result.IsSuccess
                ? TypedResults.Ok(result)
                : result.ToProblem();
        })
        .WithName("GetMunicipalities")
        .WithTags("Municipality")
        .WithSummary("Get Municipalities")
        .RequireAuthorization("Employee")
        .Produces(StatusCodes.Status200OK)
        .ProducesValidationProblem()
        .ProducesProblem(StatusCodes.Status401Unauthorized)
        .ProducesProblem(StatusCodes.Status403Forbidden);
    }

    public class Handler(PmmsDbContext context, IUserContext userContext) : IRequestHandler<Query, AppResult<Response>>
    {
        public async Task<AppResult<Response>> Handle(Query query, CancellationToken ct)
        {
            IQueryable<Municipality> dbQuery = context.Municipalities
                .AsNoTracking()
                .Include(m => m.Province);

            if (query.ByAssignmentOnly)
            {
                var provinceIds = userContext.AssignedProvinceIds;

                dbQuery = dbQuery.Where(m => provinceIds.Contains(m.ProvinceId));
            }

            var municipalities = await dbQuery
                .ProjectToType<Dto>()
                .ToListAsync(ct);

            var res = new Response(
                Municipalities: municipalities,
                Message: "Municipalities are fetched successfully."
            );
            
            return AppResult<Response>.Success(res);
        }
    }
}