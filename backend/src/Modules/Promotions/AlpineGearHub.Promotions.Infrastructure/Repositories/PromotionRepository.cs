using AlpineGearHub.Promotions.Domain.Entities;
using AlpineGearHub.Promotions.Domain.Enums;
using AlpineGearHub.Promotions.Domain.Repositories;
using AlpineGearHub.Promotions.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace AlpineGearHub.Promotions.Infrastructure.Repositories;

internal sealed class PromotionRepository(PromotionsDbContext db) : IPromotionRepository
{
    public async Task<Promotion?> GetByIdAsync(Guid id, CancellationToken ct = default) =>
        await db.Promotions.FirstOrDefaultAsync(p => p.Id == id, ct);

    public async Task<Promotion?> GetByStripePaymentIntentIdAsync(string stripePaymentIntentId, CancellationToken ct = default) =>
        await db.Promotions.FirstOrDefaultAsync(p => p.StripePaymentIntentId == stripePaymentIntentId, ct);

    public async Task<IReadOnlyList<Promotion>> GetByListingIdAsync(Guid listingId, CancellationToken ct = default) =>
        await db.Promotions
            .Where(p => p.ListingId == listingId)
            .OrderByDescending(p => p.StartAt)
            .ToListAsync(ct);

    public async Task<bool> HasActivePromotionAsync(Guid listingId, CancellationToken ct = default)
    {
        var now = DateTime.UtcNow;
        return await db.Promotions.AnyAsync(p =>
            p.ListingId == listingId &&
            p.EndAt > now &&
            (p.PaymentStatus == PaymentStatus.Pending || p.PaymentStatus == PaymentStatus.Completed), ct);
    }

    public async Task AddAsync(Promotion promotion, CancellationToken ct = default) =>
        await db.Promotions.AddAsync(promotion, ct);

    public Task SaveChangesAsync(CancellationToken ct = default) =>
        db.SaveChangesAsync(ct);
}
