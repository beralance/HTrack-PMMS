using FluentValidation;

namespace PMMS.Server.Features.Drafts.SaveDraft;

public class Validator : AbstractValidator<SaveDraftCommand>
{
    public Validator()
    {
        RuleFor(x => x.Id).NotEmpty();
    }
} 