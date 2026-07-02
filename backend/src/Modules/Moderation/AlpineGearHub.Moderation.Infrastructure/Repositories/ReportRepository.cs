using AlpineGearHub.Moderation.Domain.Entities;
using AlpineGearHub.Moderation.Domain.Repositories;
using AlpineGearHub.Moderation.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace AlpineGearHub.Moderation.Infrastructure.Repositories;

internal sealed class ReportRepository(ModerationDbContext db) : IReportRepository
{
    public async Task<Report?> GetByIdAsync(Guid id, CancellationToken ct = default) =>
        await db.Reports.FirstOrDefaultAsync(r => r.Id == id, ct);

    public async Task<(IReadOnlyList<Report> Items, int TotalCount)> GetPagedAsync(
        ReportFilter filter, CancellationToken ct = default)
    {
        var query = db.Reports.AsQueryable();

        if (filter.Status.HasValue)
            query = query.Where(r => r.Status == filter.Status.Value);

        var totalCount = await query.CountAsync(ct);

        var items = await query
            .OrderByDescending(r => r.CreatedAt)
            .Skip((filter.Page - 1) * filter.PageSize)
            .Take(filter.PageSize)
            .ToListAsync(ct);

        return (items, totalCount);
    }

    public async Task AddAsync(Report report, CancellationToken ct = default) =>
        await db.Reports.AddAsync(report, ct);

    public Task SaveChangesAsync(CancellationToken ct = default) =>
        db.SaveChangesAsync(ct);
}
