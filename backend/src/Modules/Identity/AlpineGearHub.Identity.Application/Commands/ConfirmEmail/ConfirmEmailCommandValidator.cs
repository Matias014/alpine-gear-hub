using FluentValidation;

namespace AlpineGearHub.Identity.Application.Commands.ConfirmEmail;

public sealed class ConfirmEmailCommandValidator : AbstractValidator<ConfirmEmailCommand>
{
    public ConfirmEmailCommandValidator()
    {
        RuleFor(x => x.Token).NotEmpty().WithMessage("Confirmation token is required.");
    }
}
