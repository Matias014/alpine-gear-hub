using AlpineGearHub.SharedKernel;

namespace AlpineGearHub.Identity.Domain.Entities;

public class EmailConfirmationToken : Entity
{
    public Guid UserId { get; private set; }
    public string TokenHash { get; private set; } = string.Empty;
    public DateTime ExpiresAt { get; private set; }
    public DateTime CreatedAt { get; private set; }
    public bool IsUsed { get; private set; }

    public bool IsExpired => DateTime.UtcNow >= ExpiresAt;
    public bool IsActive => !IsUsed && !IsExpired;

    private EmailConfirmationToken() { }

    public static EmailConfirmationToken Create(Guid userId, string tokenHash, DateTime expiresAt) =>
        new()
        {
            Id = Guid.NewGuid(),
            UserId = userId,
            TokenHash = tokenHash,
            ExpiresAt = expiresAt,
            CreatedAt = DateTime.UtcNow,
            IsUsed = false,
        };

    public void MarkUsed() => IsUsed = true;
}
