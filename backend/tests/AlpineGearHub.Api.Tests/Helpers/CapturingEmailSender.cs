using System.Collections.Concurrent;
using AlpineGearHub.Identity.Application.Interfaces;

namespace AlpineGearHub.Api.Tests.Helpers;

// Replaces the real (log-only) IEmailSender in tests so we can read back the actual tokens that
// would have been emailed, instead of scraping application logs.
public sealed class CapturingEmailSender : IEmailSender
{
    private readonly ConcurrentDictionary<string, string> _lastResetTokenByEmail = new();
    private readonly ConcurrentDictionary<string, string> _lastConfirmationTokenByEmail = new();

    public Task SendPasswordResetEmailAsync(string toEmail, string resetToken, CancellationToken ct = default)
    {
        _lastResetTokenByEmail[toEmail] = resetToken;
        return Task.CompletedTask;
    }

    public Task SendEmailConfirmationEmailAsync(string toEmail, string confirmationToken, CancellationToken ct = default)
    {
        _lastConfirmationTokenByEmail[toEmail] = confirmationToken;
        return Task.CompletedTask;
    }

    public string GetLastResetToken(string email) =>
        _lastResetTokenByEmail.TryGetValue(email, out var token)
            ? token
            : throw new InvalidOperationException($"No password reset email was sent to '{email}'.");

    public string GetLastConfirmationToken(string email) =>
        _lastConfirmationTokenByEmail.TryGetValue(email, out var token)
            ? token
            : throw new InvalidOperationException($"No confirmation email was sent to '{email}'.");
}
