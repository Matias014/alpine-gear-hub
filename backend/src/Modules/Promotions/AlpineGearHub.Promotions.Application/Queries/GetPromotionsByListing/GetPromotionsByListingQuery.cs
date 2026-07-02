using AlpineGearHub.Promotions.Application.DTOs;
using MediatR;

namespace AlpineGearHub.Promotions.Application.Queries.GetPromotionsByListing;

public record GetPromotionsByListingQuery(Guid ListingId) : IRequest<IReadOnlyList<PromotionResponse>>;
