using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;
using PMMS.Server.Common.Endpoints;
using PMMS.Server.Common.Result;
using PMMS.Server.Domain.Enums;
using PMMS.Server.Infrastructure.Persistence;

namespace PMMS.Server.Features.CentralProjectNetwork;

public sealed class GetCpnProjectById : IEndpoint
{
    public record Query(Guid Id) : IRequest<AppResult<Response>>;

    public record Response(
        Dto Project,
        string Message
    );

    public record Dto(
        Guid Id,
        DateOnly DateIssued,
        string ProjectName,
        string Developer,
        string CrNo,
        string LsNo,
        string Barangay,
        string Salable,
        bool IsFm,
        DateOnly CocDate,
        DateOnly DodDate,
        ProjectStatuses Status,
        string AddedById,
        MunicipalityDto Municipality,
        ProvinceDto Province,
        ProjectTypeDto ProjectType,
        DateTimeOffset CreatedAt
    );

    public record MunicipalityDto(int Id, string MunicipalityName);
    public record ProvinceDto(int Id, string ProvinceName);
    public record ProjectTypeDto(int Id, string Type, string Meaning);

    public class Validator : AbstractValidator<Query>
    {
        public Validator()
        {
            RuleFor(x => x.Id).NotEmpty().WithMessage("Project ID is required.");
        }
    }

    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapGet("/central-project-network/{id:guid}", async (Guid id, ISender sender, CancellationToken ct) =>
            {
                var result = await sender.Send(new Query(id), ct);
                
                return result.IsSuccess
                    ? TypedResults.Ok(result)
                    : result.ToProblem();
            })
            .WithName("GetCpnProjectById")
            .WithTags("Central Project Network")
            .WithSummary("Get CPN Project by Id")
            .RequireAuthorization("PermanentOnly")
            .Produces(StatusCodes.Status200OK)
            .ProducesValidationProblem()
            .ProducesProblem(StatusCodes.Status404NotFound);
    }

    public class Handler(PmmsDbContext context) : IRequestHandler<Query, AppResult<Response>>
    {
        public async Task<AppResult<Response>> Handle(Query query, CancellationToken ct)
        {
            var projectDto = await context.Projects
                .AsNoTracking()
                .Where(p => p.Id == query.Id && !p.IsDeleted && p.SetupStatus == ProjectSetupStatuses.Active)
                .Select(p => new Dto(
                    p.Id,
                    p.DateIssued,
                    p.ProjectName,
                    p.Developer,
                    p.CrNo,
                    p.LsNo,
                    p.Barangay,
                    p.Salable,
                    p.IsFm,
                    p.CocDate ?? default,
                    p.DodDate ?? default,
                    p.Status ?? default,
                    p.AddedById,
                    new MunicipalityDto(p.MunicipalityId, p.Municipality.MunicipalityName),
                    new ProvinceDto(p.Municipality.ProvinceId, p.Municipality.Province.ProvinceName),
                    new ProjectTypeDto(p.ProjectTypeId, p.ProjectType.Type, p.ProjectType.Meaning),
                    p.CreatedAt
                ))
                .FirstOrDefaultAsync(ct);

            if (projectDto is null)
            {
                return AppResult<Response>.Failure($"Project {query.Id} was not found.", ErrorType.NotFound);
            }

            var response = new Response(
                Project: projectDto,
                Message: "Project details fetched successfully."
            );

            return AppResult<Response>.Success(response);
        }
    }
}
