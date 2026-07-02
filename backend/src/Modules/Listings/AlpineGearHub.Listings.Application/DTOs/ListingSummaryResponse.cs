namespace AlpineGearHub.Listings.Application.DTOs;

public record ListingSummaryResponse(
    Guid Id,
    Guid SellerId,
    string Title,
    decimal Price,
    string Currency,
    string Condition,
    string Status,
    string Location,
    bool IsPromoted,
    string? PrimaryImageUrl,
    DateTime CreatedAt);
