using MediatR;

namespace AlpineGearHub.Identity.Application.Commands.ConfirmEmail;

// IRequest<Unit>, not bare IRequest - see the comment on RequestPasswordResetCommand for why.
public record ConfirmEmailCommand(string Token) : IRequest<Unit>;
