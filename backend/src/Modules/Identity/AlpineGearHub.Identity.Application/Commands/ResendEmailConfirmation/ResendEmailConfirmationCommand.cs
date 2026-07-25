using MediatR;

namespace AlpineGearHub.Identity.Application.Commands.ResendEmailConfirmation;

// IRequest<Unit>, not bare IRequest - see the comment on RequestPasswordResetCommand for why.
public record ResendEmailConfirmationCommand(string Email) : IRequest<Unit>;
