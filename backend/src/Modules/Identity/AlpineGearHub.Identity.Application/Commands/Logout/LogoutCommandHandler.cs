using System.Security.Cryptography;
using System.Text;
using AlpineGearHub.Identity.Domain.Repositories;
using MediatR;

namespace AlpineGearHub.Identity.Application.Commands.Logout;

internal sealed class LogoutCommandHandler(IUserRepository userRepository) : IRequestHandler<LogoutCommand, Unit>
{
    public async Task<Unit> Handle(LogoutCommand request, CancellationToken cancellationToken)
    {
        // No cookie, unknown token, or already revoked - logout is idempotent either way, there's
        // nothing sensitive in "you're already not logged in" to protect against.
        if (string.IsNullOrEmpty(request.RefreshToken)) return Unit.Value;

        var tokenHash = HashToken(request.RefreshToken);
        var user = await userRepository.GetByRefreshTokenHashAsync(tokenHash, cancellationToken);
        if (user is null) return Unit.Value;

        user.RevokeAllRefreshTokens();
        await userRepository.SaveChangesAsync(cancellationToken);

        return Unit.Value;
    }

    private static string HashToken(string rawToken)
    {
        var bytes = SHA256.HashData(Encoding.UTF8.GetBytes(rawToken));
        return Convert.ToBase64String(bytes);
    }
}
