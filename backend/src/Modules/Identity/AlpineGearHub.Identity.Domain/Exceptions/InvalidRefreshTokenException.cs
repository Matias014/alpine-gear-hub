using AlpineGearHub.SharedKernel.Exceptions;

namespace AlpineGearHub.Identity.Domain.Exceptions;

public sealed class InvalidRefreshTokenException()
    : DomainException("Refresh token is invalid or has expired.");
