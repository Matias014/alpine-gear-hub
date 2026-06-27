using AlpineGearHub.Listings.Domain.Enums;
using AlpineGearHub.SharedKernel.Exceptions;

namespace AlpineGearHub.Listings.Domain.Exceptions;

public sealed class InvalidListingStatusTransitionException(ListingStatus from, ListingStatus to)
    : DomainException($"Cannot transition listing from '{from}' to '{to}'.");
