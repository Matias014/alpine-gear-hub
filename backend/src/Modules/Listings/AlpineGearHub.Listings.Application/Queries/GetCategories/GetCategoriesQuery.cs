using AlpineGearHub.Listings.Application.DTOs;
using MediatR;

namespace AlpineGearHub.Listings.Application.Queries.GetCategories;

public record GetCategoriesQuery : IRequest<IReadOnlyList<CategoryResponse>>;
