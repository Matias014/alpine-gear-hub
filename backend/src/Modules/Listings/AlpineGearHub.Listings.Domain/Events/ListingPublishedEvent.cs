using AlpineGearHub.SharedKernel;

namespace AlpineGearHub.Listings.Domain.Events;

public sealed record ListingPublishedEvent(Guid ListingId, Guid SellerId) : IDomainEvent;
