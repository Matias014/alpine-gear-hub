using AlpineGearHub.Promotions.Domain.Enums;

namespace AlpineGearHub.Promotions.Domain;

public static class PromotionPricing
{
    public static (decimal Amount, int DurationDays) For(PromotionTier tier) => tier switch
    {
        PromotionTier.Standard => (5.00m, 7),
        PromotionTier.Featured => (15.00m, 14),
        _ => throw new ArgumentOutOfRangeException(nameof(tier)),
    };
}
