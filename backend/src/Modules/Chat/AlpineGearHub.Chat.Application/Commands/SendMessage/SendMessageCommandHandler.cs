using AlpineGearHub.Chat.Application.DTOs;
using AlpineGearHub.Chat.Application.Extensions;
using AlpineGearHub.Chat.Application.Interfaces;
using AlpineGearHub.Chat.Domain.Repositories;
using MediatR;

namespace AlpineGearHub.Chat.Application.Commands.SendMessage;

internal sealed class SendMessageCommandHandler(
    IConversationRepository conversationRepository,
    IChatNotifier chatNotifier) : IRequestHandler<SendMessageCommand, MessageResponse>
{
    public async Task<MessageResponse> Handle(SendMessageCommand request, CancellationToken cancellationToken)
    {
        var conversation = await conversationRepository.GetByIdAsync(request.ConversationId, cancellationToken)
            ?? throw new InvalidOperationException($"Conversation '{request.ConversationId}' not found.");

        if (!conversation.HasParticipant(request.SenderId))
            throw new UnauthorizedAccessException("Only the buyer or seller can send messages in this conversation.");

        var message = conversation.AddMessage(request.SenderId, request.Body);
        await conversationRepository.SaveChangesAsync(cancellationToken);

        var response = message.ToResponse();
        var recipientId = request.SenderId == conversation.BuyerId ? conversation.SellerId : conversation.BuyerId;
        await chatNotifier.NotifyMessageSentAsync(recipientId, response, cancellationToken);

        return response;
    }
}
