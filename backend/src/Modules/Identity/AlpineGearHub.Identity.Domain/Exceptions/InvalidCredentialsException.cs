using AlpineGearHub.SharedKernel.Exceptions;

namespace AlpineGearHub.Identity.Domain.Exceptions;

public sealed class InvalidCredentialsException()
    : DomainException("Invalid email or password.");
