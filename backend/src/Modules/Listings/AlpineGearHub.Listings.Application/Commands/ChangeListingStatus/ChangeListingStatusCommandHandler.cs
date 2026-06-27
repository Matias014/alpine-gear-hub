using AlpineGearHub.Listings.Domain.Repositories;
using MediatR;

namespace AlpineGearHub.Listings.Application.Commands.ChangeListingStatus;

internal sealed class ChangeListingStatusCommandHandler(IListingRepository listingRepository)
    : IRequestHandler<ChangeListingStatusCommand>
{
    public async Task Handle(ChangeListingStatusCommand request, CancellationToken cancellationToken)
    {
        var listing = await listingRepository.GetByIdAsync(request.ListingId, cancellationToken)
            ?? throw new InvalidOperationException($"Listing '{request.ListingId}' not found.");

        var isSeller = listing.SellerId == request.RequesterId;

        switch (request.Action)
        {
            case ListingStatusAction.Reserve:
                if (!isSeller) throw new UnauthorizedAccessException("Only the seller can reserve a listing.");
                listing.MarkAsReserved();
                break;

            case ListingStatusAction.Sell:
                if (!isSeller) throw new UnauthorizedAccessException("Only the seller can mark a listing as sold.");
                listing.MarkAsSold();
                break;

            case ListingStatusAction.Renew:
                if (!isSeller) throw new UnauthorizedAccessException("Only the seller can renew a listing.");
                listing.Renew();
                break;

            case ListingStatusAction.Remove:
                if (!isSeller && !request.IsAdminOrModerator)
                    throw new UnauthorizedAccessException("Only the seller, moderator, or admin can remove a listing.");
                listing.Remove();
                break;
        }

        await listingRepository.SaveChangesAsync(cancellationToken);
    }
}
