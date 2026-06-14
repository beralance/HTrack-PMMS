using FluentValidation;

namespace PMMS.Server.Features.Expirations.ResetExpiration;

public sealed class Validator : AbstractValidator<ResetExpirationCommand>
{
    public Validator() 
    {
        RuleFor(x => x.Id)
            .NotEmpty().WithMessage("Project ID is required.");

        RuleFor(x => x.DateOfCompletion)
            .NotEmpty().WithMessage("Date of Completion is required.")
            .NotEqual(default(DateOnly)).WithMessage("A valid Date of Completion must be provided.");

        RuleFor(x => x.ExtensionOfTime)
            .GreaterThanOrEqualTo(x => x.DateOfCompletion)
            .When(x => x.ExtensionOfTime.HasValue)
            .WithMessage("Extension of Time must be on or after the original Date of Completion.");
    }
}