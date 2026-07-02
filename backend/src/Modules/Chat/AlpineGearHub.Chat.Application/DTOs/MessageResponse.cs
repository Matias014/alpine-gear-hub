namespace AlpineGearHub.Chat.Application.DTOs;

public record MessageResponse(
    Guid Id,
    Guid ConversationId,
    Guid SenderId,
    string Body,
    DateTime SentAt,
    DateTime? ReadAt);
