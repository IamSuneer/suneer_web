namespace suneer_web.Services;

public interface IEmailService
{
    Task SendPasswordResetCodeAsync(string toEmail, string code);
}
