using AlpineGearHub.Promotions.Application.DTOs;
using AlpineGearHub.Promotions.Domain.Entities;

namespace AlpineGearHub.Promotions.Application.Extensions;

internal static class PromotionMappingExtensions
{
    public static PromotionResponse ToResponse(this Promotion promotion, string? clientSecret = null) =>
        new(
            promotion.Id,
            promotion.ListingId,
            promotion.Tier.ToString(),
            promotion.StartAt,
            promotion.EndAt,
            promotion.Price.Amount,
            promotion.Price.Currency,
            promotion.PaymentStatus.ToString(),
            promotion.StripePaymentIntentId,
            clientSecret);
}
