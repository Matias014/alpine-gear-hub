using AlpineGearHub.Listings.Domain.Repositories;
using MediatR;

namespace AlpineGearHub.Listings.Application.Commands.PublishListing;

internal sealed class PublishListingCommandHandler(IListingRepository listingRepository)
    : IRequestHandler<PublishListingCommand>
{
    public async Task Handle(PublishListingCommand request, CancellationToken cancellationToken)
    {
        var listing = await listingRepository.GetByIdAsync(request.ListingId, cancellationToken)
            ?? throw new InvalidOperationException($"Listing '{request.ListingId}' not found.");

        if (listing.SellerId != request.RequesterId)
            throw new UnauthorizedAccessException("Only the seller can publish this listing.");

        listing.Publish();
        await listingRepository.SaveChangesAsync(cancellationToken);
    }
}
