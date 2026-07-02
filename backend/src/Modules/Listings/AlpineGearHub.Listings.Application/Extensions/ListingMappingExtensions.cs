using AlpineGearHub.Listings.Application.DTOs;
using AlpineGearHub.Listings.Application.Interfaces;
using AlpineGearHub.Listings.Domain.Entities;

namespace AlpineGearHub.Listings.Application.Extensions;

internal static class ListingMappingExtensions
{
    public static ListingResponse ToResponse(this Listing listing, string categoryName, IFileStorage fileStorage) =>
        new(
            listing.Id,
            listing.SellerId,
            listing.CategoryId,
            categoryName,
            listing.Title,
            listing.Description,
            listing.Price.Amount,
            listing.Price.Currency,
            listing.Condition.ToString(),
            listing.Status.ToString(),
            listing.Location,
            listing.IsPromoted,
            listing.CreatedAt,
            listing.ExpiresAt,
            listing.Images
                .OrderBy(i => i.SortOrder)
                .Select(i => new ListingImageResponse(i.Id, fileStorage.GetPublicUrl(i.StorageKey), i.SortOrder, i.IsPrimary))
                .ToList());
}
