namespace AlpineGearHub.Identity.Application.Interfaces;

public interface IEmailSender
{
    Task SendPasswordResetEmailAsync(string toEmail, string resetToken, CancellationToken ct = default);
    Task SendEmailConfirmationEmailAsync(string toEmail, string confirmationToken, CancellationToken ct = default);
}
