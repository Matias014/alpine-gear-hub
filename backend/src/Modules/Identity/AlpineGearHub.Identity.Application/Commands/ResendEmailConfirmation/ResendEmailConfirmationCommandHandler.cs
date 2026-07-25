using AlpineGearHub.Identity.Application.Interfaces;
using AlpineGearHub.Identity.Domain.Repositories;
using MediatR;

namespace AlpineGearHub.Identity.Application.Commands.ResendEmailConfirmation;

internal sealed class ResendEmailConfirmationCommandHandler(
    IUserRepository userRepository,
    ITokenService tokenService,
    IEmailSender emailSender)
    : IRequestHandler<ResendEmailConfirmationCommand, Unit>
{
    public async Task<Unit> Handle(ResendEmailConfirmationCommand request, CancellationToken cancellationToken)
    {
        var user = await userRepository.GetByEmailAsync(request.Email, cancellationToken);

        // Same "always behave the same" story as password reset - no free oracle for whether an
        // email is registered, and nothing to resend once it's already confirmed.
        if (user is null || user.EmailConfirmed) return Unit.Value;

        var (rawToken, tokenHash, expiresAt) = tokenService.GenerateEmailConfirmationToken();
        user.AddEmailConfirmationToken(tokenHash, expiresAt);

        await userRepository.SaveChangesAsync(cancellationToken);
        await emailSender.SendEmailConfirmationEmailAsync(user.Email, rawToken, cancellationToken);

        return Unit.Value;
    }
}
