using AlpineGearHub.Listings.Domain.Repositories;
using MediatR;

namespace AlpineGearHub.Listings.Application.Commands.SetListingPromoted;

internal sealed class SetListingPromotedCommandHandler(IListingRepository listingRepository)
    : IRequestHandler<SetListingPromotedCommand>
{
    public async Task Handle(SetListingPromotedCommand request, CancellationToken cancellationToken)
    {
        var listing = await listingRepository.GetByIdAsync(request.ListingId, cancellationToken)
            ?? throw new InvalidOperationException($"Listing '{request.ListingId}' not found.");

        listing.SetPromoted(request.IsPromoted);
        await listingRepository.SaveChangesAsync(cancellationToken);
    }
}
