using MediatR;

namespace AlpineGearHub.Identity.Application.Commands.Logout;

// IRequest<Unit>, not bare IRequest - see the comment on RequestPasswordResetCommand for why.
public record LogoutCommand(string? RefreshToken) : IRequest<Unit>;
