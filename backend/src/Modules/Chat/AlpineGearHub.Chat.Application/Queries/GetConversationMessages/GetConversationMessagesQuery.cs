using AlpineGearHub.Chat.Application.DTOs;
using MediatR;

namespace AlpineGearHub.Chat.Application.Queries.GetConversationMessages;

public record GetConversationMessagesQuery(Guid ConversationId, Guid RequesterId) : IRequest<IReadOnlyList<MessageResponse>>;
