using AlpineGearHub.Identity.Application.DTOs;
using AlpineGearHub.Identity.Application.Interfaces;
using AlpineGearHub.Identity.Domain.Entities;
using AlpineGearHub.Identity.Domain.Exceptions;
using AlpineGearHub.Identity.Domain.Repositories;
using MediatR;
using Microsoft.AspNetCore.Identity;

namespace AlpineGearHub.Identity.Application.Commands.Register;

public sealed class RegisterCommandHandler(
    IUserRepository userRepository,
    IPasswordHasher<User> passwordHasher,
    ITokenService tokenService,
    IEmailSender emailSender)
    : IRequestHandler<RegisterCommand, AuthResponse>
{
    public async Task<AuthResponse> Handle(RegisterCommand request, CancellationToken cancellationToken)
    {
        var existing = await userRepository.GetByEmailAsync(request.Email, cancellationToken);
        if (existing is not null)
            throw new EmailAlreadyTakenException(request.Email);

        var passwordHash = passwordHasher.HashPassword(null!, request.Password);
        var user = User.Create(request.Email, request.FullName, passwordHash);

        await userRepository.AddAsync(user, cancellationToken);

        var accessToken = tokenService.GenerateAccessToken(user);
        var (rawRefresh, refreshHash, refreshExpiry) = tokenService.GenerateRefreshToken();
        user.AddRefreshToken(refreshHash, refreshExpiry);

        var (rawConfirmation, confirmationHash, confirmationExpiry) = tokenService.GenerateEmailConfirmationToken();
        user.AddEmailConfirmationToken(confirmationHash, confirmationExpiry);

        await userRepository.SaveChangesAsync(cancellationToken);
        await emailSender.SendEmailConfirmationEmailAsync(user.Email, rawConfirmation, cancellationToken);

        return new AuthResponse(
            accessToken,
            DateTime.UtcNow.AddMinutes(15),
            rawRefresh,
            user.FullName,
            user.Email,
            user.Role.ToString());
    }
}
