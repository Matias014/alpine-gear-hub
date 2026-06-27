using AlpineGearHub.Listings.Application.DTOs;
using AlpineGearHub.Listings.Application.Extensions;
using AlpineGearHub.Listings.Application.Interfaces;
using AlpineGearHub.Listings.Domain.Repositories;
using MediatR;

namespace AlpineGearHub.Listings.Application.Queries.GetListingById;

internal sealed class GetListingByIdQueryHandler(
    IListingRepository listingRepository,
    ICategoryRepository categoryRepository,
    IFileStorage fileStorage) : IRequestHandler<GetListingByIdQuery, ListingResponse?>
{
    public async Task<ListingResponse?> Handle(GetListingByIdQuery request, CancellationToken cancellationToken)
    {
        var listing = await listingRepository.GetByIdAsync(request.ListingId, cancellationToken);
        if (listing is null) return null;

        var category = await categoryRepository.GetByIdAsync(listing.CategoryId, cancellationToken);
        return listing.ToResponse(category?.Name ?? string.Empty, fileStorage);
    }
}
