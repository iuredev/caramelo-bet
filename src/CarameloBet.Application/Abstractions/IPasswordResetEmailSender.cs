namespace CarameloBet.Application.Abstractions;

public interface IPasswordResetEmailSender
{
    Task SendAsync(string email, string resetToken);
}
