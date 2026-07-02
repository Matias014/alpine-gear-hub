using AlpineGearHub.Chat.Application.DTOs;
using AlpineGearHub.Chat.Application.Extensions;
using AlpineGearHub.Chat.Domain.Repositories;
using MediatR;

namespace AlpineGearHub.Chat.Application.Queries.GetConversationMessages;

internal sealed class GetConversationMessagesQueryHandler(IConversationRepository conversationRepository)
    : IRequestHandler<GetConversationMessagesQuery, IReadOnlyList<MessageResponse>>
{
    public async Task<IReadOnlyList<MessageResponse>> Handle(
        GetConversationMessagesQuery request, CancellationToken cancellationToken)
    {
        var conversation = await conversationRepository.GetByIdAsync(request.ConversationId, cancellationToken)
            ?? throw new InvalidOperationException($"Conversation '{request.ConversationId}' not found.");

        if (!conversation.HasParticipant(request.RequesterId))
            throw new UnauthorizedAccessException("Only the buyer or seller can view this conversation.");

        return conversation.Messages
            .OrderBy(m => m.SentAt)
            .Select(m => m.ToResponse())
            .ToList();
    }
}
