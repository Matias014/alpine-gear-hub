using AlpineGearHub.Listings.Domain.Entities;

namespace AlpineGearHub.Listings.Domain.Repositories;

public record ListingFilter(
    Guid? CategoryId = null,
    string? Condition = null,
    decimal? MinPrice = null,
    decimal? MaxPrice = null,
    string? Search = null,
    Guid? SellerId = null,
    int Page = 1,
    int PageSize = 20);

public interface IListingRepository
{
    Task<Listing?> GetByIdAsync(Guid id, CancellationToken ct = default);
    Task<(IReadOnlyList<Listing> Items, int TotalCount)> GetPagedAsync(ListingFilter filter, CancellationToken ct = default);
    Task AddAsync(Listing listing, CancellationToken ct = default);
    Task SaveChangesAsync(CancellationToken ct = default);
}
