using AlpineGearHub.Identity.Domain.Entities;

namespace AlpineGearHub.Identity.Application.Interfaces;

// Shared with Program.cs's "RequireConfirmedEmail" authorization policy, so both sides agree on
// the claim name without the Host needing to reach into TokenService's internals.
public static class AuthClaimTypes
{
    public const string EmailVerified = "email_verified";
}

public interface ITokenService
{
    string GenerateAccessToken(User user);
    (string RawToken, string TokenHash, DateTime ExpiresAt) GenerateRefreshToken();
    (string RawToken, string TokenHash, DateTime ExpiresAt) GeneratePasswordResetToken();
    (string RawToken, string TokenHash, DateTime ExpiresAt) GenerateEmailConfirmationToken();
}
