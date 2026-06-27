using MediatR;

namespace AlpineGearHub.Listings.Application.Commands.ChangeListingStatus;

public enum ListingStatusAction { Reserve, Sell, Renew, Remove }

public record ChangeListingStatusCommand(
    Guid ListingId,
    Guid RequesterId,
    bool IsAdminOrModerator,
    ListingStatusAction Action) : IRequest;
