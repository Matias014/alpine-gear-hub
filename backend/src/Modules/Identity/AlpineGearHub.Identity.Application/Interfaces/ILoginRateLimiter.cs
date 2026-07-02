namespace AlpineGearHub.Identity.Application.Interfaces;

public interface ILoginRateLimiter
{
    Task<bool> IsBlockedAsync(string email, CancellationToken ct = default);
    Task RegisterFailedAttemptAsync(string email, CancellationToken ct = default);
    Task ResetAsync(string email, CancellationToken ct = default);
}
