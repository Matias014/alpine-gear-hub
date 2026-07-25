using AlpineGearHub.Identity.Application.Interfaces;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace AlpineGearHub.Identity.Infrastructure.Services;

// No real email provider wired up yet (same "placeholder" story as Stripe's dev key) - this just
// logs the link so the reset flow is fully usable/testable without needing real SMTP infra. Swap
// this out for a real sender (SendGrid/MailKit/etc.) before going to production.
public sealed class LoggingEmailSender(IConfiguration configuration, ILogger<LoggingEmailSender> logger)
    : IEmailSender
{
    public Task SendPasswordResetEmailAsync(string toEmail, string resetToken, CancellationToken ct = default)
    {
        var frontendBaseUrl = configuration["Cors:AllowedOrigins"] ?? "http://localhost:3000";
        var resetLink = $"{frontendBaseUrl}/reset-password?token={Uri.EscapeDataString(resetToken)}";

        logger.LogInformation("Password reset link for {Email}: {ResetLink}", toEmail, resetLink);
        return Task.CompletedTask;
    }
}
