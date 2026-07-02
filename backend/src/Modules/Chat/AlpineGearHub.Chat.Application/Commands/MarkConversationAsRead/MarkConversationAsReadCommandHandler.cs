using AlpineGearHub.Chat.Domain.Repositories;
using MediatR;

namespace AlpineGearHub.Chat.Application.Commands.MarkConversationAsRead;

internal sealed class MarkConversationAsReadCommandHandler(IConversationRepository conversationRepository)
    : IRequestHandler<MarkConversationAsReadCommand>
{
    public async Task Handle(MarkConversationAsReadCommand request, CancellationToken cancellationToken)
    {
        var conversation = await conversationRepository.GetByIdAsync(request.ConversationId, cancellationToken)
            ?? throw new InvalidOperationException($"Conversation '{request.ConversationId}' not found.");

        if (!conversation.HasParticipant(request.RequesterId))
            throw new UnauthorizedAccessException("Only the buyer or seller can mark this conversation as read.");

        conversation.MarkMessagesAsRead(request.RequesterId);
        await conversationRepository.SaveChangesAsync(cancellationToken);
    }
}
