using FluentValidation;

namespace AlpineGearHub.Moderation.Application.Commands.CreateReport;

public sealed class CreateReportCommandValidator : AbstractValidator<CreateReportCommand>
{
    public CreateReportCommandValidator()
    {
        RuleFor(x => x.ListingId).NotEmpty().WithMessage("Listing is required.");
        RuleFor(x => x.ReportedByUserId).NotEmpty().WithMessage("Reporter is required.");
        RuleFor(x => x.Description)
            .MaximumLength(1000).WithMessage("Description must not exceed 1000 characters.");
    }
}
