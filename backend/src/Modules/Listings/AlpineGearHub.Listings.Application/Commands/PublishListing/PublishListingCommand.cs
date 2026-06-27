using MediatR;

namespace AlpineGearHub.Listings.Application.Commands.PublishListing;

public record PublishListingCommand(Guid ListingId, Guid RequesterId) : IRequest;
