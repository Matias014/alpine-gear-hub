namespace AlpineGearHub.Chat.Application.DTOs;

public record ConversationSummaryResponse(
    Guid Id,
    Guid ListingId,
    Guid OtherParticipantId,
    string? LastMessageBody,
    DateTime? LastMessageAt,
    int UnreadCount);
