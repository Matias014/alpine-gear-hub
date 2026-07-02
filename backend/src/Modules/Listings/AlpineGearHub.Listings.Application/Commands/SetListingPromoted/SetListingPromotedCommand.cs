using MediatR;

namespace AlpineGearHub.Listings.Application.Commands.SetListingPromoted;

public record SetListingPromotedCommand(Guid ListingId, bool IsPromoted) : IRequest;
