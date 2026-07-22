using AlpineGearHub.Listings.Application.DTOs;
using AlpineGearHub.Listings.Application.Extensions;
using AlpineGearHub.Listings.Application.Interfaces;
using AlpineGearHub.Listings.Domain.Entities;
using AlpineGearHub.Listings.Domain.Repositories;
using AlpineGearHub.SharedKernel;
using MediatR;

namespace AlpineGearHub.Listings.Application.Commands.CreateListing;

internal sealed class CreateListingCommandHandler(
    IListingRepository listingRepository,
    ICategoryRepository categoryRepository,
    IFileStorage fileStorage) : IRequestHandler<CreateListingCommand, ListingResponse>
{
    public async Task<ListingResponse> Handle(CreateListingCommand request, CancellationToken cancellationToken)
    {
        var category = await categoryRepository.GetByIdAsync(request.CategoryId, cancellationToken)
            ?? throw new InvalidOperationException($"Category '{request.CategoryId}' not found.");

        var listing = Listing.Create(
            request.SellerId,
            request.CategoryId,
            request.Title,
            request.Description,
            Money.Of(request.Price, request.Currency),
            request.Condition,
            request.Location);

        await listingRepository.AddAsync(listing, cancellationToken);
        await listingRepository.SaveChangesAsync(cancellationToken);

        return listing.ToResponse(category.Name, fileStorage);
    }
}
