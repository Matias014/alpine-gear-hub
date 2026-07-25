using AlpineGearHub.Identity.Domain.Enums;
using AlpineGearHub.SharedKernel;

namespace AlpineGearHub.Identity.Domain.Entities;

public class User : AggregateRoot
{
    private readonly List<RefreshToken> _refreshTokens = [];
    private readonly List<PasswordResetToken> _passwordResetTokens = [];
    private readonly List<EmailConfirmationToken> _emailConfirmationTokens = [];

    public string Email { get; private set; } = string.Empty;
    public string PasswordHash { get; private set; } = string.Empty;
    public string FullName { get; private set; } = string.Empty;
    public UserRole Role { get; private set; }
    public bool EmailConfirmed { get; private set; }
    public DateTime CreatedAt { get; private set; }

    public IReadOnlyList<RefreshToken> RefreshTokens => _refreshTokens.AsReadOnly();
    public IReadOnlyList<PasswordResetToken> PasswordResetTokens => _passwordResetTokens.AsReadOnly();
    public IReadOnlyList<EmailConfirmationToken> EmailConfirmationTokens => _emailConfirmationTokens.AsReadOnly();

    private User() { }

    public static User Create(
        string email,
        string fullName,
        string passwordHash,
        UserRole role = UserRole.Member,
        bool emailConfirmed = false)
    {
        return new User
        {
            Id = Guid.NewGuid(),
            Email = email.Trim().ToLowerInvariant(),
            FullName = fullName.Trim(),
            PasswordHash = passwordHash,
            Role = role,
            EmailConfirmed = emailConfirmed,
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

    public void RevokeAllRefreshTokens()
    {
        foreach (var token in _refreshTokens.Where(t => t.IsActive))
            token.Revoke();
    }

    public PasswordResetToken AddPasswordResetToken(string tokenHash, DateTime expiresAt)
    {
        // Requesting a new reset link should invalidate any still-outstanding one, same as
        // AddRefreshToken does for logins - otherwise an old, already-superseded link stays usable.
        foreach (var existing in _passwordResetTokens.Where(t => t.IsActive))
            existing.MarkUsed();

        var token = PasswordResetToken.Create(Id, tokenHash, expiresAt);
        _passwordResetTokens.Add(token);
        return token;
    }

    public PasswordResetToken? FindActivePasswordResetToken(string tokenHash) =>
        _passwordResetTokens.FirstOrDefault(t => t.TokenHash == tokenHash && t.IsActive);

    public void SetPassword(string newPasswordHash) => PasswordHash = newPasswordHash;

    public EmailConfirmationToken AddEmailConfirmationToken(string tokenHash, DateTime expiresAt)
    {
        // Same story as password reset tokens - a new confirmation link should supersede any
        // still-outstanding one.
        foreach (var existing in _emailConfirmationTokens.Where(t => t.IsActive))
            existing.MarkUsed();

        var token = EmailConfirmationToken.Create(Id, tokenHash, expiresAt);
        _emailConfirmationTokens.Add(token);
        return token;
    }

    public EmailConfirmationToken? FindActiveEmailConfirmationToken(string tokenHash) =>
        _emailConfirmationTokens.FirstOrDefault(t => t.TokenHash == tokenHash && t.IsActive);

    public void ConfirmEmail() => EmailConfirmed = true;
}
