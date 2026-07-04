using FluentValidation;
using MediatR;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;
using Microsoft.EntityFrameworkCore;
using PMMS.Server.Common.Endpoints;
using PMMS.Server.Common.Result;
using PMMS.Server.Common.Security;
using PMMS.Server.Domain.Enums;
using PMMS.Server.Infrastructure.Persistence;

namespace PMMS.Server.Features.Notification;

public sealed class DeleteNotification : IEndpoint
{
    public record Command(int Id) : IRequest<AppResult<Response>>;

    public record Response(
        int Id,
        string Message  
    );
    public class Validator : AbstractValidator<Command> 
    {
        public Validator()
        {
            RuleFor(x => x.Id)
                .GreaterThan(0).WithMessage("A valid notification identifier is required.");
        }
    }

    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapDelete("notifications/{id:int}", async ([FromRoute] int id, ISender sender, CancellationToken ct) =>
        {
            var result = await sender.Send(new Command(id), ct);
            
            return result.IsSuccess ? TypedResults.NoContent() : result.ToProblem();
        })
        .WithName("DeleteNotification")
        .WithTags("Notifications")
        .RequireAuthorization();
    }

    public class Handler(PmmsDbContext context, IUserContext userContext) 
        : IRequestHandler<Command, AppResult<Response>>
    {
        public async Task<AppResult<Response>> Handle(Command command, CancellationToken ct)
        {
            var currentUserId = userContext.UserId;
            
            var notification = await context.Notifications
                .FirstOrDefaultAsync(n => n.Id == command.Id, ct);

            if (notification is null)
            {
                return AppResult<Response>.Failure($"Notification {command.Id} was not found.", ErrorType.NotFound);
            }

            if (notification.UserId != currentUserId)
            {
                return AppResult<Response>.Failure("You do not have permission to alter this record.", ErrorType.Forbidden);
            }

            notification.IsDeleted = true;
            notification.DeletedAt = DateTimeOffset.UtcNow;
            notification.DeletedById = currentUserId;

            await context.SaveChangesAsync(ct);
            
            var res = new Response(
                Id: notification.Id,
                Message: "Notification successfully deleted."
            );
            return AppResult<Response>.Success(res);
        }
    }
}