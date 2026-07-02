using AlpineGearHub.SharedKernel;

namespace AlpineGearHub.Chat.Domain.Events;

public sealed record MessageSentEvent(
    Guid ConversationId,
    Guid MessageId,
    Guid SenderId,
    Guid RecipientId,
    string Body,
    DateTime SentAt) : IDomainEvent;
