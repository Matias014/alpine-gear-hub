using FluentValidation;

namespace AlpineGearHub.Chat.Application.Commands.SendMessage;

public sealed class SendMessageCommandValidator : AbstractValidator<SendMessageCommand>
{
    public SendMessageCommandValidator()
    {
        RuleFor(x => x.ConversationId).NotEmpty().WithMessage("Conversation is required.");
        RuleFor(x => x.SenderId).NotEmpty().WithMessage("Sender is required.");
        RuleFor(x => x.Body)
            .NotEmpty().WithMessage("Message body is required.")
            .MaximumLength(2000).WithMessage("Message must not exceed 2000 characters.");
    }
}
