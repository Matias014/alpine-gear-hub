using AlpineGearHub.Chat.Application.DTOs;
using AlpineGearHub.Chat.Application.Extensions;
using AlpineGearHub.Chat.Domain.Repositories;
using MediatR;

namespace AlpineGearHub.Chat.Application.Queries.GetConversations;

internal sealed class GetConversationsQueryHandler(IConversationRepository conversationRepository)
    : IRequestHandler<GetConversationsQuery, IReadOnlyList<ConversationSummaryResponse>>
{
    public async Task<IReadOnlyList<ConversationSummaryResponse>> Handle(
        GetConversationsQuery request, CancellationToken cancellationToken)
    {
        var conversations = await conversationRepository.GetForUserAsync(request.UserId, cancellationToken);

        return conversations
            .OrderByDescending(c => c.LastMessageAt ?? c.CreatedAt)
            .Select(c => c.ToSummaryResponse(request.UserId))
            .ToList();
    }
}
