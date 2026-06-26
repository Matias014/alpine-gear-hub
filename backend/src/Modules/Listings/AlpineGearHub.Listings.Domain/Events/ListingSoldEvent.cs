using AlpineGearHub.SharedKernel;

namespace AlpineGearHub.Listings.Domain.Events;

public sealed record ListingSoldEvent(Guid ListingId, Guid SellerId) : IDomainEvent;
