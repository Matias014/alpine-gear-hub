using AlpineGearHub.SharedKernel.Exceptions;

namespace AlpineGearHub.Identity.Domain.Exceptions;

public sealed class InvalidEmailConfirmationTokenException()
    : DomainException("Email confirmation link is invalid or has expired.");
