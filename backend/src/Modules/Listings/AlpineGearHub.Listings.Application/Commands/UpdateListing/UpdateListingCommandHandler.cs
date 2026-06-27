using AlpineGearHub.Listings.Application.DTOs;
using AlpineGearHub.Listings.Application.Extensions;
using AlpineGearHub.Listings.Application.Interfaces;
using AlpineGearHub.Listings.Domain.Repositories;
using AlpineGearHub.Listings.Domain.ValueObjects;
using MediatR;

namespace AlpineGearHub.Listings.Application.Commands.UpdateListing;

internal sealed class UpdateListingCommandHandler(
    IListingRepository listingRepository,
    ICategoryRepository categoryRepository,
    IFileStorage fileStorage) : IRequestHandler<UpdateListingCommand, ListingResponse>
{
    public async Task<ListingResponse> Handle(UpdateListingCommand request, CancellationToken cancellationToken)
    {
        var listing = await listingRepository.GetByIdAsync(request.ListingId, cancellationToken)
            ?? throw new InvalidOperationException($"Listing '{request.ListingId}' not found.");

        if (listing.SellerId != request.RequesterId)
            throw new UnauthorizedAccessException("Only the seller can update this listing.");

        listing.Update(request.Title, request.Description,
            Money.Of(request.Price, request.Currency), request.Condition, request.Location);

        await listingRepository.SaveChangesAsync(cancellationToken);

        var category = await categoryRepository.GetByIdAsync(listing.CategoryId, cancellationToken);
        return listing.ToResponse(category?.Name ?? string.Empty, fileStorage);
    }
}
