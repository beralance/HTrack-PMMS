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
