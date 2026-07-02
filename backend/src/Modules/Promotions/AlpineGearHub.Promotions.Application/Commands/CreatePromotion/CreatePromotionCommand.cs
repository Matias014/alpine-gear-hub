using AlpineGearHub.Promotions.Application.DTOs;
using AlpineGearHub.Promotions.Domain.Enums;
using MediatR;

namespace AlpineGearHub.Promotions.Application.Commands.CreatePromotion;

public record CreatePromotionCommand(Guid ListingId, PromotionTier Tier) : IRequest<PromotionResponse>;
