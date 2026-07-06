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
        // The declared Content-Type header is client-supplied and therefore untrusted - only the
        // actual file signature decides what gets stored and served back as.
        var detectedContentType = await ImageSignature.DetectContentTypeAsync(request.Content, cancellationToken)
            ?? throw new InvalidOperationException("Only JPEG, PNG and WebP images are allowed.");

        var listing = await listingRepository.GetByIdAsync(request.ListingId, cancellationToken)
            ?? throw new InvalidOperationException($"Listing '{request.ListingId}' not found.");

        if (listing.SellerId != request.RequesterId)
            throw new UnauthorizedAccessException("Only the seller can upload images to this listing.");

        var extension = Path.GetExtension(request.FileName);
        var storageKey = $"listings/{request.ListingId}/{Guid.NewGuid()}{extension}";

        await fileStorage.UploadAsync(request.Content, storageKey, detectedContentType, cancellationToken);

        var image = listing.AddImage(storageKey);
        await listingRepository.SaveChangesAsync(cancellationToken);

        return new ListingImageResponse(image.Id, fileStorage.GetPublicUrl(storageKey), image.SortOrder, image.IsPrimary);
    }
}
