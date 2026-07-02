using FluentValidation;

namespace AlpineGearHub.Chat.Application.Commands.StartConversation;

public sealed class StartConversationCommandValidator : AbstractValidator<StartConversationCommand>
{
    public StartConversationCommandValidator()
    {
        RuleFor(x => x.ListingId).NotEmpty().WithMessage("Listing is required.");
        RuleFor(x => x.BuyerId).NotEmpty().WithMessage("Buyer is required.");
        RuleFor(x => x.SellerId).NotEmpty().WithMessage("Seller is required.");
    }
}
