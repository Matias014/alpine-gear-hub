using AlpineGearHub.Listings.Application.Interfaces;
using AlpineGearHub.Listings.Domain.Repositories;
using MediatR;

namespace AlpineGearHub.Listings.Application.Commands.DeleteListingImage;

internal sealed class DeleteListingImageCommandHandler(
    IListingRepository listingRepository,
    IFileStorage fileStorage) : IRequestHandler<DeleteListingImageCommand>
{
    public async Task Handle(DeleteListingImageCommand request, CancellationToken cancellationToken)
    {
        var listing = await listingRepository.GetByIdAsync(request.ListingId, cancellationToken)
            ?? throw new InvalidOperationException($"Listing '{request.ListingId}' not found.");

        if (listing.SellerId != request.RequesterId)
            throw new UnauthorizedAccessException("Only the seller can delete images from this listing.");

        var image = listing.Images.FirstOrDefault(i => i.Id == request.ImageId)
            ?? throw new InvalidOperationException($"Image '{request.ImageId}' not found.");

        await fileStorage.DeleteAsync(image.StorageKey, cancellationToken);
        listing.RemoveImage(request.ImageId);
        await listingRepository.SaveChangesAsync(cancellationToken);
    }
}
