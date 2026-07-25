using System.Collections.Concurrent;
using AlpineGearHub.Identity.Application.Interfaces;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace AlpineGearHub.Identity.Infrastructure.Services;

// No real email provider wired up yet (same "placeholder" story as Stripe's dev key) - this just
// logs the link so the reset/confirmation flows are fully usable/testable without needing real
// SMTP infra. Swap this out for a real sender (SendGrid/MailKit/etc.) before going to production.
public sealed class LoggingEmailSender(IConfiguration configuration, ILogger<LoggingEmailSender> logger)
    : IEmailSender
{
    // Dev-only convenience: lets a human (or scripts/seed-demo-listings.sh) fetch the confirmation
    // link without a real inbox, via GET /api/auth/dev/last-confirmation-link (see Program.cs,
    // only mapped when IsDevelopment()). Never touches the DB - the raw token is only ever stored
    // hashed there, same as refresh/reset tokens.
    private static readonly ConcurrentDictionary<string, string> LastConfirmationLinkByEmail = new();

    public Task SendPasswordResetEmailAsync(string toEmail, string resetToken, CancellationToken ct = default)
    {
        var frontendBaseUrl = configuration["Cors:AllowedOrigins"] ?? "http://localhost:3000";
        var resetLink = $"{frontendBaseUrl}/reset-password?token={Uri.EscapeDataString(resetToken)}";

        logger.LogInformation("Password reset link for {Email}: {ResetLink}", toEmail, resetLink);
        return Task.CompletedTask;
    }

    public Task SendEmailConfirmationEmailAsync(string toEmail, string confirmationToken, CancellationToken ct = default)
    {
        var frontendBaseUrl = configuration["Cors:AllowedOrigins"] ?? "http://localhost:3000";
        var confirmationLink = $"{frontendBaseUrl}/confirm-email?token={Uri.EscapeDataString(confirmationToken)}";

        LastConfirmationLinkByEmail[toEmail] = confirmationLink;
        logger.LogInformation("Email confirmation link for {Email}: {ConfirmationLink}", toEmail, confirmationLink);
        return Task.CompletedTask;
    }

    public static string? GetLastConfirmationLink(string email) =>
        LastConfirmationLinkByEmail.GetValueOrDefault(email);
}
