using AlpineGearHub.Listings.Application.DTOs;
using AlpineGearHub.Listings.Domain.Enums;
using MediatR;

namespace AlpineGearHub.Listings.Application.Commands.CreateListing;

public record CreateListingCommand(
    Guid SellerId,
    Guid CategoryId,
    string Title,
    string Description,
    decimal Price,
    string Currency,
    GearCondition Condition,
    string Location) : IRequest<ListingResponse>;
