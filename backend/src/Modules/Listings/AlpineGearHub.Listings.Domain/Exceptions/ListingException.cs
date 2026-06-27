using AlpineGearHub.SharedKernel.Exceptions;

namespace AlpineGearHub.Listings.Domain.Exceptions;

public sealed class ListingException(string message) : DomainException(message);
