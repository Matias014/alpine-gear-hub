using AlpineGearHub.Identity.Domain.Entities;

namespace AlpineGearHub.Identity.Application.Interfaces;

public interface ITokenService
{
    string GenerateAccessToken(User user);
    (string RawToken, string TokenHash, DateTime ExpiresAt) GenerateRefreshToken();
}
