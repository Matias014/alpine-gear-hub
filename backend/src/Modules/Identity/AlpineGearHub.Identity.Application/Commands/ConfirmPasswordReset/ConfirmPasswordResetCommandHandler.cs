using System.Security.Cryptography;
using System.Text;
using AlpineGearHub.Identity.Domain.Entities;
using AlpineGearHub.Identity.Domain.Exceptions;
using AlpineGearHub.Identity.Domain.Repositories;
using MediatR;
using Microsoft.AspNetCore.Identity;

namespace AlpineGearHub.Identity.Application.Commands.ConfirmPasswordReset;

internal sealed class ConfirmPasswordResetCommandHandler(
    IUserRepository userRepository,
    IPasswordHasher<User> passwordHasher)
    : IRequestHandler<ConfirmPasswordResetCommand, Unit>
{
    public async Task<Unit> Handle(ConfirmPasswordResetCommand request, CancellationToken cancellationToken)
    {
        var tokenHash = HashToken(request.Token);

        var user = await userRepository.GetByPasswordResetTokenHashAsync(tokenHash, cancellationToken);
        var resetToken = user?.FindActivePasswordResetToken(tokenHash);
        if (user is null || resetToken is null)
            throw new InvalidPasswordResetTokenException();

        var newPasswordHash = passwordHasher.HashPassword(null!, request.NewPassword);
        user.SetPassword(newPasswordHash);
        resetToken.MarkUsed();

        // A password reset is also a good moment to kill any sessions started with the old,
        // possibly-compromised password.
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
