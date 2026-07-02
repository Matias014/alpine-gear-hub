using AlpineGearHub.Chat.Application.DTOs;
using AlpineGearHub.Chat.Domain.Entities;

namespace AlpineGearHub.Chat.Application.Extensions;

internal static class ChatMappingExtensions
{
    public static ConversationResponse ToResponse(this Conversation conversation) =>
        new(
            conversation.Id,
            conversation.ListingId,
            conversation.BuyerId,
            conversation.SellerId,
            conversation.CreatedAt,
            conversation.LastMessageAt);

    public static MessageResponse ToResponse(this Message message) =>
        new(message.Id, message.ConversationId, message.SenderId, message.Body, message.SentAt, message.ReadAt);

    public static ConversationSummaryResponse ToSummaryResponse(this Conversation conversation, Guid currentUserId)
    {
        var otherParticipantId = currentUserId == conversation.BuyerId ? conversation.SellerId : conversation.BuyerId;
        var lastMessage = conversation.Messages.OrderByDescending(m => m.SentAt).FirstOrDefault();
        var unreadCount = conversation.Messages.Count(m => m.SenderId != currentUserId && m.ReadAt is null);

        return new ConversationSummaryResponse(
            conversation.Id,
            conversation.ListingId,
            otherParticipantId,
            lastMessage?.Body,
            conversation.LastMessageAt,
            unreadCount);
    }
}
