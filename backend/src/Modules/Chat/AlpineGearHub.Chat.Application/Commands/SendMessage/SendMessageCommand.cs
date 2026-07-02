using AlpineGearHub.Chat.Application.DTOs;
using MediatR;

namespace AlpineGearHub.Chat.Application.Commands.SendMessage;

public record SendMessageCommand(Guid ConversationId, Guid SenderId, string Body) : IRequest<MessageResponse>;
