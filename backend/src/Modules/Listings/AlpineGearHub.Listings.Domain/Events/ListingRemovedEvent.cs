using AlpineGearHub.SharedKernel;

namespace AlpineGearHub.Listings.Domain.Events;

public sealed record ListingRemovedEvent(Guid ListingId, Guid RemovedByUserId) : IDomainEvent;
