using AlpineGearHub.SharedKernel.Exceptions;

namespace AlpineGearHub.Moderation.Domain.Exceptions;

public sealed class ModerationException(string message) : DomainException(message);
