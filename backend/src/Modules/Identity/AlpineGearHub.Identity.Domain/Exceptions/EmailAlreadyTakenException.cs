using AlpineGearHub.SharedKernel.Exceptions;

namespace AlpineGearHub.Identity.Domain.Exceptions;

public sealed class EmailAlreadyTakenException(string email)
    : DomainException($"Email '{email}' is already registered.");
