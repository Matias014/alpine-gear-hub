using AlpineGearHub.Chat.Application.DTOs;
using AlpineGearHub.Chat.Application.Extensions;
using AlpineGearHub.Chat.Domain.Entities;
using AlpineGearHub.Chat.Domain.Repositories;
using MediatR;

namespace AlpineGearHub.Chat.Application.Commands.StartConversation;

internal sealed class StartConversationCommandHandler(IConversationRepository conversationRepository)
    : IRequestHandler<StartConversationCommand, ConversationResponse>
{
    public async Task<ConversationResponse> Handle(StartConversationCommand request, CancellationToken cancellationToken)
    {
        // A buyer can only have one thread per listing, so re-opening it returns the existing conversation.
        var existing = await conversationRepository.GetByListingAndBuyerAsync(
            request.ListingId, request.BuyerId, cancellationToken);
        if (existing is not null) return existing.ToResponse();

        var conversation = Conversation.Create(request.ListingId, request.BuyerId, request.SellerId);
        await conversationRepository.AddAsync(conversation, cancellationToken);
        await conversationRepository.SaveChangesAsync(cancellationToken);

        return conversation.ToResponse();
    }
}
