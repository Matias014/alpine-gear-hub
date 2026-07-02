using AlpineGearHub.Listings.Domain.Entities;
using AlpineGearHub.Listings.Domain.Enums;
using AlpineGearHub.Listings.Domain.Repositories;
using AlpineGearHub.Listings.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace AlpineGearHub.Listings.Infrastructure.Repositories;

internal sealed class ListingRepository(ListingsDbContext db) : IListingRepository
{
    public async Task<Listing?> GetByIdAsync(Guid id, CancellationToken ct = default) =>
        await db.Listings
            .Include(l => l.Images)
            .FirstOrDefaultAsync(l => l.Id == id, ct);

    public async Task<(IReadOnlyList<Listing> Items, int TotalCount)> GetPagedAsync(
        ListingFilter filter, CancellationToken ct = default)
    {
        var query = db.Listings
            .Include(l => l.Images)
            .Where(l => l.Status == ListingStatus.Active || l.Status == ListingStatus.Reserved)
            .AsQueryable();

        if (filter.CategoryId.HasValue)
            query = query.Where(l => l.CategoryId == filter.CategoryId.Value);

        if (filter.SellerId.HasValue)
            query = query.Where(l => l.SellerId == filter.SellerId.Value);

        if (!string.IsNullOrWhiteSpace(filter.Condition) &&
            Enum.TryParse<GearCondition>(filter.Condition, ignoreCase: true, out var condition))
            query = query.Where(l => l.Condition == condition);

        if (filter.MinPrice.HasValue)
            query = query.Where(l => l.Price.Amount >= filter.MinPrice.Value);

        if (filter.MaxPrice.HasValue)
            query = query.Where(l => l.Price.Amount <= filter.MaxPrice.Value);

        if (!string.IsNullOrWhiteSpace(filter.Search))
        {
            var term = filter.Search.ToLower();
            query = query.Where(l =>
                EF.Functions.ILike(l.Title, $"%{term}%") ||
                EF.Functions.ILike(l.Description, $"%{term}%"));
        }

        var totalCount = await query.CountAsync(ct);

        var items = await query
            .OrderByDescending(l => l.IsPromoted)
            .ThenByDescending(l => l.CreatedAt)
            .Skip((filter.Page - 1) * filter.PageSize)
            .Take(filter.PageSize)
            .ToListAsync(ct);

        return (items, totalCount);
    }

    public async Task AddAsync(Listing listing, CancellationToken ct = default) =>
        await db.Listings.AddAsync(listing, ct);

    public Task SaveChangesAsync(CancellationToken ct = default) =>
        db.SaveChangesAsync(ct);
}
