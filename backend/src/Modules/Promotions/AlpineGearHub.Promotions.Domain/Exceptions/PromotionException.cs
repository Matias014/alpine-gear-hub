using AlpineGearHub.SharedKernel.Exceptions;

namespace AlpineGearHub.Promotions.Domain.Exceptions;

public sealed class PromotionException(string message) : DomainException(message);
