using MediatR;

namespace AlpineGearHub.Listings.Application.Commands.DeleteListingImage;

public record DeleteListingImageCommand(Guid ListingId, Guid ImageId, Guid RequesterId) : IRequest;
