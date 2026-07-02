using AlpineGearHub.Moderation.Domain.Entities;
using AlpineGearHub.Moderation.Domain.Enums;

namespace AlpineGearHub.Moderation.Domain.Repositories;

public record ReportFilter(ReportStatus? Status = null, int Page = 1, int PageSize = 20);

public interface IReportRepository
{
    Task<Report?> GetByIdAsync(Guid id, CancellationToken ct = default);
    Task<(IReadOnlyList<Report> Items, int TotalCount)> GetPagedAsync(ReportFilter filter, CancellationToken ct = default);
    Task AddAsync(Report report, CancellationToken ct = default);
    Task SaveChangesAsync(CancellationToken ct = default);
}
