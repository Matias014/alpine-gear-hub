using AlpineGearHub.SharedKernel.Exceptions;

namespace AlpineGearHub.Identity.Domain.Exceptions;

public sealed class TooManyLoginAttemptsException()
    : DomainException("Too many failed login attempts. Try again later.");
