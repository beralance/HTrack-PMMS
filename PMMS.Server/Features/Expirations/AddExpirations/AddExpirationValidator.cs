using FluentValidation;

namespace PMMS.Server.Features.Expirations.AddExpirations;

public class AddExpirationValidator : AbstractValidator<AddExpirationCommand>
{
    public AddExpirationValidator() 
    {
        RuleFor(x => x.ProjectId)
            .NotEmpty().WithMessage("Project ID is required.");

        RuleFor(x => x.ExtensionOfTime)
            .GreaterThanOrEqualTo(x => x.DateOfCompletion)
            .When(x => x.ExtensionOfTime.HasValue)
            .WithMessage("Extension of Time must be on or after the original Date of Completion.");
    }
}