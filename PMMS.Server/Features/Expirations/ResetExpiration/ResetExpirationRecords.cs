using MediatR;
using PMMS.Server.Common.Result;

namespace PMMS.Server.Features.Expirations.ResetExpiration;

public sealed record ResetExpirationCommand(
        Guid Id,
        DateOnly? CocDate,
        DateOnly? DodDate,
        bool? HasCoc,
        bool? HasDod,

        DateOnly DateOfCompletion,
        DateOnly? ExtensionOfTime
    ) : IRequest<AppResult<ResetExpirationResponse>>;

public sealed record ResetExpirationResponse(
    Guid Id,
    string Message
);


// Completed: system
// Ongoing: sytem
// cancelled: system
// expired: system

/*
    Expired Status logic:
    cron job
    if expiration status greater than date today
    mark expiration as expired

    OnGoing Status Logic:
    if user request a handleProject,
    change the expiration status to ongoing

    Cancelled Status Logic:
    if a project is cancelled, mark the expirations as cancelled
    no manual usage of cancellation
    Cancel should be inside the project management, instead of expirations

    Completed status logic:
    MarkAsCompleted Request


    MarkExpirationAsHandled
*/ 