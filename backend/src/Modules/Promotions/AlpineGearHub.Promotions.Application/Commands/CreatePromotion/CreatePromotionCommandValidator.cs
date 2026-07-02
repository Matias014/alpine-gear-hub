using FluentValidation;

namespace AlpineGearHub.Promotions.Application.Commands.CreatePromotion;

public sealed class CreatePromotionCommandValidator : AbstractValidator<CreatePromotionCommand>
{
    public CreatePromotionCommandValidator()
    {
        RuleFor(x => x.ListingId).NotEmpty().WithMessage("Listing is required.");
    }
}
