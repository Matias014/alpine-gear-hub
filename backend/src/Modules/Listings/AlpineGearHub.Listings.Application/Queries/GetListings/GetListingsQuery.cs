using AlpineGearHub.Listings.Application.DTOs;
using MediatR;

namespace AlpineGearHub.Listings.Application.Queries.GetListings;

public record GetListingsQuery(
    Guid? CategoryId = null,
    string? Condition = null,
    decimal? MinPrice = null,
    decimal? MaxPrice = null,
    string? Search = null,
    Guid? SellerId = null,
    int Page = 1,
    int PageSize = 20) : IRequest<PagedResponse<ListingSummaryResponse>>;
