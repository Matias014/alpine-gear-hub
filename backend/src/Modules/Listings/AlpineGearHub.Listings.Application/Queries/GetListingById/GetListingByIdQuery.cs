using AlpineGearHub.Listings.Application.DTOs;
using MediatR;

namespace AlpineGearHub.Listings.Application.Queries.GetListingById;

public record GetListingByIdQuery(Guid ListingId) : IRequest<ListingResponse?>;
