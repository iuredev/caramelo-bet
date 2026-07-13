using CarameloBet.Application.Abstractions;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Resend;

namespace CarameloBet.Infrastructure.Services.Auth;

public class PasswordResetEmailSender(
    IConfiguration configuration,
    ILogger<PasswordResetEmailSender> logger,
    IResend resend) : IPasswordResetEmailSender
{
    public async Task SendAsync(string email, string resetToken)
    {
        var frontendUrl = configuration["Frontend:BaseUrl"] ?? "http://localhost:3000";
        var resetUrl = $"{frontendUrl}/reset-password?token={Uri.EscapeDataString(resetToken)}";
        var apiToken = configuration["Resend:ApiToken"];

        if (string.IsNullOrWhiteSpace(apiToken))
        {
            logger.LogWarning(
                "Password reset email was not sent because Resend:ApiToken is not configured.");

            return;
        }

        var message = new EmailMessage
        {
            From = configuration["Resend:FromEmail"] ?? "CarameloBet <onboarding@resend.dev>",
            Subject = "Reset your CarameloBet password",
            HtmlBody = $"""
                <p>Use the link below to reset your CarameloBet password.</p>
                <p><a href="{resetUrl}">Reset password</a></p>
                <p>If you did not request this, you can ignore this email.</p>
                """,
            TextBody = $"""
                Use the link below to reset your CarameloBet password.

                {resetUrl}

                If you did not request this, you can ignore this email.
                """
        };

        message.To.Add(email);

        await resend.EmailSendAsync(message);
    }
}
