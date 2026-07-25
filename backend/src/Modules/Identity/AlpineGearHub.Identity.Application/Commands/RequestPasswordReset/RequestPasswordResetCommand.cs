using MediatR;

namespace AlpineGearHub.Identity.Application.Commands.RequestPasswordReset;

// Using IRequest<Unit> instead of bare IRequest here - I found the hard way that
// ValidationBehavior never gets constructed for bare-IRequest commands (validators.Any() debug
// print never even fired), so the validator below was silently skipped. Spelling out Unit fixes it.
public record RequestPasswordResetCommand(string Email) : IRequest<Unit>;
