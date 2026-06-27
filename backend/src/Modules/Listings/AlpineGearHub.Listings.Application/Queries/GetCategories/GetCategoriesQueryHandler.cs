using AlpineGearHub.Listings.Application.DTOs;
using AlpineGearHub.Listings.Application.Interfaces;
using AlpineGearHub.Listings.Domain.Repositories;
using MediatR;

namespace AlpineGearHub.Listings.Application.Queries.GetCategories;

internal sealed class GetCategoriesQueryHandler(
    ICategoryRepository categoryRepository,
    ICacheService cacheService) : IRequestHandler<GetCategoriesQuery, IReadOnlyList<CategoryResponse>>
{
    private const string CacheKey = "categories:all";
    private static readonly TimeSpan CacheExpiry = TimeSpan.FromHours(1);

    public async Task<IReadOnlyList<CategoryResponse>> Handle(GetCategoriesQuery request, CancellationToken cancellationToken)
    {
        var cached = await cacheService.GetAsync<List<CategoryResponse>>(CacheKey, cancellationToken);
        if (cached is not null) return cached;

        var categories = await categoryRepository.GetAllAsync(cancellationToken);
        var result = categories.Select(c => new CategoryResponse(c.Id, c.Name, c.Slug)).ToList();

        await cacheService.SetAsync(CacheKey, result, CacheExpiry, cancellationToken);
        return result;
    }
}
