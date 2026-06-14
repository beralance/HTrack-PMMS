using MediatR;
using PMMS.Server.Common.Result;

namespace PMMS.Server.Features.Expirations.AddExpirations;

public record AddExpirationRequest (
    DateOnly? CocDate,
    DateOnly? DodDate,
    DateOnly DateOfCompletion,
    DateOnly? ExtensionOfTime
);

public record AddExpirationCommand(
    Guid ProjectId,
    DateOnly? CocDate,
    DateOnly? DodDate,
    bool? HasCoc,
    bool? HasDod,

    DateOnly DateOfCompletion,
    DateOnly? ExtensionOfTime
) : IRequest<AppResult<AddExpirationResponse>>;

public record AddExpirationResponse(
    Guid Id,
    string Message
);
