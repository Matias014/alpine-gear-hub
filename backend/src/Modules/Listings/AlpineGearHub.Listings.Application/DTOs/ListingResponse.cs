namespace AlpineGearHub.Listings.Application.DTOs;

public record ListingResponse(
    Guid Id,
    Guid SellerId,
    Guid CategoryId,
    string CategoryName,
    string Title,
    string Description,
    decimal Price,
    string Currency,
    string Condition,
    string Status,
    string Location,
    bool IsPromoted,
    DateTime CreatedAt,
    DateTime? ExpiresAt,
    IReadOnlyList<ListingImageResponse> Images);
