using System.Security.Cryptography;
using System.Text;
using AlpineGearHub.Identity.Application.DTOs;
using AlpineGearHub.Identity.Application.Interfaces;
using AlpineGearHub.Identity.Domain.Exceptions;
using AlpineGearHub.Identity.Domain.Repositories;
using MediatR;

namespace AlpineGearHub.Identity.Application.Commands.RefreshToken;

public sealed class RefreshTokenCommandHandler(
    IUserRepository userRepository,
    ITokenService tokenService)
    : IRequestHandler<RefreshTokenCommand, AuthResponse>
{
    public async Task<AuthResponse> Handle(RefreshTokenCommand request, CancellationToken cancellationToken)
    {
        var tokenHash = HashToken(request.RefreshToken);

        var user = await userRepository.GetByRefreshTokenHashAsync(tokenHash, cancellationToken);
        if (user is null)
            throw new InvalidRefreshTokenException();

        var existingToken = user.FindActiveRefreshToken(tokenHash);
        if (existingToken is null)
            throw new InvalidRefreshTokenException();

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

    private static string HashToken(string rawToken)
    {
        var bytes = SHA256.HashData(Encoding.UTF8.GetBytes(rawToken));
        return Convert.ToBase64String(bytes);
    }
}
