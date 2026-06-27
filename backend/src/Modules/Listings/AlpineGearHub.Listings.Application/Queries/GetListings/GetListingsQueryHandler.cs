using AlpineGearHub.Listings.Application.DTOs;
using AlpineGearHub.Listings.Application.Interfaces;
using AlpineGearHub.Listings.Domain.Repositories;
using MediatR;

namespace AlpineGearHub.Listings.Application.Queries.GetListings;

internal sealed class GetListingsQueryHandler(
    IListingRepository listingRepository,
    IFileStorage fileStorage) : IRequestHandler<GetListingsQuery, PagedResponse<ListingSummaryResponse>>
{
    public async Task<PagedResponse<ListingSummaryResponse>> Handle(GetListingsQuery request, CancellationToken cancellationToken)
    {
        var filter = new ListingFilter(
            request.CategoryId,
            request.Condition,
            request.MinPrice,
            request.MaxPrice,
            request.Search,
            request.SellerId,
            request.Page,
            request.PageSize);

        var (items, totalCount) = await listingRepository.GetPagedAsync(filter, cancellationToken);

        var summaries = items.Select(l =>
        {
            var primaryImage = l.Images.FirstOrDefault(i => i.IsPrimary)?.StorageKey
                ?? l.Images.FirstOrDefault()?.StorageKey;

            return new ListingSummaryResponse(
                l.Id,
                l.SellerId,
                l.Title,
                l.Price.Amount,
                l.Price.Currency,
                l.Condition.ToString(),
                l.Status.ToString(),
                l.Location,
                primaryImage is not null ? fileStorage.GetPublicUrl(primaryImage) : null,
                l.CreatedAt);
        }).ToList();

        return new PagedResponse<ListingSummaryResponse>(summaries, request.Page, request.PageSize, totalCount);
    }
}
