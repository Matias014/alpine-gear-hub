using AlpineGearHub.Listings.Application.DTOs;
using MediatR;

namespace AlpineGearHub.Listings.Application.Commands.UploadListingImage;

public record UploadListingImageCommand(
    Guid ListingId,
    Guid RequesterId,
    Stream Content,
    string FileName,
    string ContentType) : IRequest<ListingImageResponse>;
