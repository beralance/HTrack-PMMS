using FluentValidation;
using Mapster;
using MediatR;
using Microsoft.EntityFrameworkCore;
using PMMS.Server.Common.Endpoints;
using PMMS.Server.Common.Result;
using PMMS.Server.Common.Security;
using PMMS.Server.Domain.Entities;
using PMMS.Server.Infrastructure.Persistence;

namespace PMMS.Server.Features.Drafts;


public sealed class GetDraftById : IEndpoint {
    
    public record Query(Guid Id, bool IncludeDeleted = false)
        : IRequest<AppResult<Response>>;

    
    public record Response (
        Dto Draft,
        string Message
    );

    public record Dto (
        Guid Id,
        DateOnly DateIssued,
        string ProjectName,
        string Developer,
        string Owner,
        string CrNo,
        string LsNo,
        string Barangay,
        string Salable,
        bool IsFm,
        Municipality Municipality,
        ProjectType ProjectType,
        string? Remarks,

        bool? HasCoc,
        bool? HasDod,
        DateOnly? CocDate,
        DateOnly? DodDate,
        DateOnly? DateOfCompletion,
        DateOnly? ExtensionOfTime,
        DateOnly? PerformanceBond,
        DateOnly? SemestralReport,

        string AddedById,
        DateTimeOffset CreatedAt
    );


    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapGet("drafts/{Id:guid}", async ([AsParameters] Query query, ISender sender, CancellationToken ct) =>
            {
                var result = await sender.Send(query, ct);

                return result.IsSuccess
                    ? TypedResults.Ok(result)
                    : result.ToProblem();
            })
            .WithName("GetDraftById")
            .WithTags("Drafts")
            .WithSummary("Get draft by id")
            .RequireAuthorization("TemporaryOnly")
            .Produces(StatusCodes.Status200OK)
            .ProducesProblem(StatusCodes.Status404NotFound);
    }


    public class Handler(PmmsDbContext context)
    : IRequestHandler<Query, AppResult<Response>>
    {
        public async Task<AppResult<Response>> Handle(Query query, CancellationToken ct)
        {
            // 1. Fetch draft with required navigation properties
            var draft = await context.Drafts
                .AsNoTracking()
                .Include(d => d.ProjectType)
                .Include(d => d.Municipality)
                    .ThenInclude(m => m.Province)
                .FirstOrDefaultAsync(d => d.Id == query.Id && d.IsPublished == false, ct);

            if (draft is null)
            {
                return AppResult<Response>.Failure($"Draft {query.Id} was not found.", ErrorType.NotFound);
            }

            // 3. Mapping
            var dto = new Dto(
                Id: draft.Id,
                DateIssued: draft.DateIssued,
                ProjectName: draft.ProjectName,
                Developer: draft.Developer,
                Owner: draft.Owner,
                CrNo: draft.CrNo,
                LsNo: draft.LsNo,
                Barangay: draft.Barangay,
                Salable: draft.Salable,
                IsFm: draft.IsFm,
                Municipality: draft.Municipality,
                ProjectType: draft.ProjectType,
                Remarks: draft.Remarks,
                HasCoc: draft.HasCoc,
                HasDod: draft.HasDod,
                CocDate: draft.CocDate,
                DodDate: draft.DodDate,
                DateOfCompletion: draft.DateOfCompletion,
                ExtensionOfTime: draft.ExtensionOfTime,
                PerformanceBond: draft.PerformanceBond,
                SemestralReport: draft.SemestralReport,
                AddedById: draft.AddedById,
                CreatedAt: draft.CreatedAt
            );

            return AppResult<Response>.Success(new Response(dto, "Draft fetched successfully."));
        }
    }
}