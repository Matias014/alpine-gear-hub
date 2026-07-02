using MediatR;

namespace AlpineGearHub.Chat.Application.Commands.MarkConversationAsRead;

public record MarkConversationAsReadCommand(Guid ConversationId, Guid RequesterId) : IRequest;
