namespace AlpineGearHub.Chat.Application.DTOs;

public record ConversationResponse(
    Guid Id,
    Guid ListingId,
    Guid BuyerId,
    Guid SellerId,
    DateTime CreatedAt,
    DateTime? LastMessageAt);
