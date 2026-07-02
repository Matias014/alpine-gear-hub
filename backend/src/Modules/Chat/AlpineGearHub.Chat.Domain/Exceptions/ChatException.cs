using AlpineGearHub.SharedKernel.Exceptions;

namespace AlpineGearHub.Chat.Domain.Exceptions;

public sealed class ChatException(string message) : DomainException(message);
