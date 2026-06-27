using AlpineGearHub.Identity.Domain.Enums;
using AlpineGearHub.SharedKernel;

namespace AlpineGearHub.Identity.Domain.Entities;

public class User : AggregateRoot
{
    private readonly List<RefreshToken> _refreshTokens = [];

    public string Email { get; private set; } = string.Empty;
    public string PasswordHash { get; private set; } = string.Empty;
    public string FullName { get; private set; } = string.Empty;
    public UserRole Role { get; private set; }
    public DateTime CreatedAt { get; private set; }

    public IReadOnlyList<RefreshToken> RefreshTokens => _refreshTokens.AsReadOnly();

    private User() { }

    public static User Create(
        string email,
        string fullName,
        string passwordHash,
        UserRole role = UserRole.Member)
    {
        return new User
        {
            Id = Guid.NewGuid(),
            Email = email.Trim().ToLowerInvariant(),
            FullName = fullName.Trim(),
            PasswordHash = passwordHash,
            Role = role,
            CreatedAt = DateTime.UtcNow,
        };
    }

    public RefreshToken AddRefreshToken(string tokenHash, DateTime expiresAt)
    {
        foreach (var existing in _refreshTokens.Where(t => t.IsActive))
            existing.Revoke();

        var token = RefreshToken.Create(Id, tokenHash, expiresAt);
        _refreshTokens.Add(token);
        return token;
    }

    public RefreshToken? FindActiveRefreshToken(string tokenHash) =>
        _refreshTokens.FirstOrDefault(t => t.TokenHash == tokenHash && t.IsActive);
}
