using AlpineGearHub.Chat.Application.DTOs;
using MediatR;

namespace AlpineGearHub.Chat.Application.Queries.GetConversations;

public record GetConversationsQuery(Guid UserId) : IRequest<IReadOnlyList<ConversationSummaryResponse>>;
