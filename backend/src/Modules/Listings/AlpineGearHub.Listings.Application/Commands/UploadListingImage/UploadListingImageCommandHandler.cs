using AlpineGearHub.Listings.Application.DTOs;
using AlpineGearHub.Listings.Application.Interfaces;
using AlpineGearHub.Listings.Domain.Repositories;
using MediatR;

namespace AlpineGearHub.Listings.Application.Commands.UploadListingImage;

internal sealed class UploadListingImageCommandHandler(
    IListingRepository listingRepository,
    IFileStorage fileStorage) : IRequestHandler<UploadListingImageCommand, ListingImageResponse>
{
    public async Task<ListingImageResponse> Handle(UploadListingImageCommand request, CancellationToken cancellationToken)
    {
        // The declared Content-Type header (and the filename/extension) are client-supplied and
        // therefore untrusted - only the actual file signature decides both what this is stored
        // and served as, and what extension the storage key gets.
        var detected = await ImageSignature.DetectAsync(request.Content, cancellationToken)
            ?? throw new InvalidOperationException("Only JPEG, PNG and WebP images are allowed.");

        var listing = await listingRepository.GetByIdAsync(request.ListingId, cancellationToken)
            ?? throw new InvalidOperationException($"Listing '{request.ListingId}' not found.");

        if (listing.SellerId != request.RequesterId)
            throw new UnauthorizedAccessException("Only the seller can upload images to this listing.");

        var storageKey = $"listings/{request.ListingId}/{Guid.NewGuid()}{detected.Extension}";

        await fileStorage.UploadAsync(request.Content, storageKey, detected.ContentType, cancellationToken);

        var image = listing.AddImage(storageKey);
        await listingRepository.SaveChangesAsync(cancellationToken);

        return new ListingImageResponse(image.Id, fileStorage.GetPublicUrl(storageKey), image.SortOrder, image.IsPrimary);
    }
}
