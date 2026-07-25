using AlpineGearHub.SharedKernel.Exceptions;

namespace AlpineGearHub.Identity.Domain.Exceptions;

public sealed class InvalidPasswordResetTokenException()
    : DomainException("Password reset link is invalid or has expired.");
