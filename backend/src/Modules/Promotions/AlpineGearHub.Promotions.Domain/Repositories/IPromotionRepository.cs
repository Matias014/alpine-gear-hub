using AlpineGearHub.Promotions.Domain.Entities;

namespace AlpineGearHub.Promotions.Domain.Repositories;

public interface IPromotionRepository
{
    Task<Promotion?> GetByIdAsync(Guid id, CancellationToken ct = default);
    Task<Promotion?> GetByStripePaymentIntentIdAsync(string stripePaymentIntentId, CancellationToken ct = default);
    Task<IReadOnlyList<Promotion>> GetByListingIdAsync(Guid listingId, CancellationToken ct = default);
    Task<bool> HasActivePromotionAsync(Guid listingId, CancellationToken ct = default);
    Task AddAsync(Promotion promotion, CancellationToken ct = default);
    Task SaveChangesAsync(CancellationToken ct = default);
}
