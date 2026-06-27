using AlpineGearHub.Identity.Application.DTOs;
using AlpineGearHub.Identity.Application.Interfaces;
using AlpineGearHub.Identity.Domain.Entities;
using AlpineGearHub.Identity.Domain.Exceptions;
using AlpineGearHub.Identity.Domain.Repositories;
using MediatR;
using Microsoft.AspNetCore.Identity;

namespace AlpineGearHub.Identity.Application.Commands.Login;

public sealed class LoginCommandHandler(
    IUserRepository userRepository,
    IPasswordHasher<User> passwordHasher,
    ITokenService tokenService)
    : IRequestHandler<LoginCommand, AuthResponse>
{
    public async Task<AuthResponse> Handle(LoginCommand request, CancellationToken cancellationToken)
    {
        var user = await userRepository.GetByEmailAsync(
            request.Email.Trim().ToLowerInvariant(), cancellationToken);

        if (user is null)
            throw new InvalidCredentialsException();

        var result = passwordHasher.VerifyHashedPassword(null!, user.PasswordHash, request.Password);
        if (result == PasswordVerificationResult.Failed)
            throw new InvalidCredentialsException();

        var accessToken = tokenService.GenerateAccessToken(user);
        var (rawRefresh, refreshHash, refreshExpiry) = tokenService.GenerateRefreshToken();
        user.AddRefreshToken(refreshHash, refreshExpiry);

        await userRepository.SaveChangesAsync(cancellationToken);

        return new AuthResponse(
            accessToken,
            DateTime.UtcNow.AddMinutes(15),
            rawRefresh,
            user.FullName,
            user.Email,
            user.Role.ToString());
    }
}
