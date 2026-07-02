namespace AlpineGearHub.Promotions.Application.DTOs;

public record PromotionResponse(
    Guid Id,
    Guid ListingId,
    string Tier,
    DateTime StartAt,
    DateTime EndAt,
    decimal Price,
    string Currency,
    string PaymentStatus,
    string? StripePaymentIntentId,
    string? ClientSecret);
