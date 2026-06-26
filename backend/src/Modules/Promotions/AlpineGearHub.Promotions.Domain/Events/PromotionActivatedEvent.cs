using AlpineGearHub.SharedKernel;

namespace AlpineGearHub.Promotions.Domain.Events;

public sealed record PromotionActivatedEvent(Guid PromotionId, Guid ListingId) : IDomainEvent;
