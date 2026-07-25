using System.Security.Cryptography;
using System.Text;
using AlpineGearHub.Identity.Domain.Exceptions;
using AlpineGearHub.Identity.Domain.Repositories;
using MediatR;

namespace AlpineGearHub.Identity.Application.Commands.ConfirmEmail;

internal sealed class ConfirmEmailCommandHandler(IUserRepository userRepository)
    : IRequestHandler<ConfirmEmailCommand, Unit>
{
    public async Task<Unit> Handle(ConfirmEmailCommand request, CancellationToken cancellationToken)
    {
        var tokenHash = HashToken(request.Token);

        var user = await userRepository.GetByEmailConfirmationTokenHashAsync(tokenHash, cancellationToken);
        var confirmationToken = user?.FindActiveEmailConfirmationToken(tokenHash);
        if (user is null || confirmationToken is null)
            throw new InvalidEmailConfirmationTokenException();

        user.ConfirmEmail();
        confirmationToken.MarkUsed();

        await userRepository.SaveChangesAsync(cancellationToken);

        return Unit.Value;
    }

    private static string HashToken(string rawToken)
    {
        var bytes = SHA256.HashData(Encoding.UTF8.GetBytes(rawToken));
        return Convert.ToBase64String(bytes);
    }
}
