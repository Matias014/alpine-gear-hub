using AlpineGearHub.Listings.Application.DTOs;
using AlpineGearHub.Listings.Domain.Enums;
using MediatR;

namespace AlpineGearHub.Listings.Application.Commands.UpdateListing;

public record UpdateListingCommand(
    Guid ListingId,
    Guid RequesterId,
    string Title,
    string Description,
    decimal Price,
    string Currency,
    GearCondition Condition,
    string Location) : IRequest<ListingResponse>;
