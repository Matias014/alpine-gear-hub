using AlpineGearHub.Listings.Domain.Entities;
using AlpineGearHub.Listings.Domain.Repositories;
using AlpineGearHub.Listings.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace AlpineGearHub.Listings.Infrastructure.Repositories;

internal sealed class CategoryRepository(ListingsDbContext db) : ICategoryRepository
{
    public async Task<IReadOnlyList<Category>> GetAllAsync(CancellationToken ct = default) =>
        await db.Categories.OrderBy(c => c.Name).ToListAsync(ct);

    public async Task<Category?> GetByIdAsync(Guid id, CancellationToken ct = default) =>
        await db.Categories.FindAsync([id], ct);

    public async Task AddAsync(Category category, CancellationToken ct = default) =>
        await db.Categories.AddAsync(category, ct);

    public Task SaveChangesAsync(CancellationToken ct = default) =>
        db.SaveChangesAsync(ct);
}
