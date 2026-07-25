using AlpineGearHub.Identity.Application.Interfaces;
using AlpineGearHub.Identity.Domain.Repositories;
using MediatR;

namespace AlpineGearHub.Identity.Application.Commands.RequestPasswordReset;

internal sealed class RequestPasswordResetCommandHandler(
    IUserRepository userRepository,
    ITokenService tokenService,
    IEmailSender emailSender)
    : IRequestHandler<RequestPasswordResetCommand, Unit>
{
    public async Task<Unit> Handle(RequestPasswordResetCommand request, CancellationToken cancellationToken)
    {
        var user = await userRepository.GetByEmailAsync(request.Email, cancellationToken);

        // Always behave the same whether the account exists or not - otherwise this endpoint
        // becomes a free "does this email have an account" oracle for anyone probing it.
        if (user is null) return Unit.Value;

        var (rawToken, tokenHash, expiresAt) = tokenService.GeneratePasswordResetToken();
        user.AddPasswordResetToken(tokenHash, expiresAt);

        await userRepository.SaveChangesAsync(cancellationToken);
        await emailSender.SendPasswordResetEmailAsync(user.Email, rawToken, cancellationToken);

        return Unit.Value;
    }
}
