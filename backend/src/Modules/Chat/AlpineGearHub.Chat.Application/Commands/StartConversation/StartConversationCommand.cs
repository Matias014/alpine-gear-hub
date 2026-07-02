using AlpineGearHub.Chat.Application.DTOs;
using MediatR;

namespace AlpineGearHub.Chat.Application.Commands.StartConversation;

public record StartConversationCommand(Guid ListingId, Guid BuyerId, Guid SellerId) : IRequest<ConversationResponse>;
