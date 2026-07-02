using AlpineGearHub.Promotions.Application.DTOs;
using AlpineGearHub.Promotions.Application.Extensions;
using AlpineGearHub.Promotions.Domain.Repositories;
using MediatR;

namespace AlpineGearHub.Promotions.Application.Queries.GetPromotionsByListing;

internal sealed class GetPromotionsByListingQueryHandler(IPromotionRepository promotionRepository)
    : IRequestHandler<GetPromotionsByListingQuery, IReadOnlyList<PromotionResponse>>
{
    public async Task<IReadOnlyList<PromotionResponse>> Handle(
        GetPromotionsByListingQuery request, CancellationToken cancellationToken)
    {
        var promotions = await promotionRepository.GetByListingIdAsync(request.ListingId, cancellationToken);
        return promotions.Select(p => p.ToResponse()).ToList();
    }
}
