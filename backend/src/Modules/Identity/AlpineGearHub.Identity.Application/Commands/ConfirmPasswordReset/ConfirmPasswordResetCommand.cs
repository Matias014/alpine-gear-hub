using MediatR;

namespace AlpineGearHub.Identity.Application.Commands.ConfirmPasswordReset;

// IRequest<Unit>, not bare IRequest - see the comment on RequestPasswordResetCommand.
public record ConfirmPasswordResetCommand(string Token, string NewPassword) : IRequest<Unit>;
