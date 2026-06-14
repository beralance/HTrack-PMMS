using MediatR;
using PMMS.Server.Common.Result;

namespace PMMS.Server.Features.Drafts.SaveDraft;

public record SaveDraftCommand (
    Guid Id
) : IRequest<AppResult<SaveDraftResponse>>;

    
public record SaveDraftResponse (
    Guid ProjectId,
    string Message
);

public record DraftCreatedEvent(
    Guid ProjectId, 
    int ProvinceId, 
    string? TemporaryId
) : INotification;
