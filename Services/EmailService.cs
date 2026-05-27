using SendGrid;
using SendGrid.Helpers.Mail;

namespace suneer_web.Services;

public class EmailService : IEmailService
{
    private readonly IConfiguration _config;
    private readonly ILogger<EmailService> _logger;

    public EmailService(IConfiguration config, ILogger<EmailService> logger)
    {
        _config = config;
        _logger = logger;
    }

    public async Task SendPasswordResetCodeAsync(string toEmail, string code)
    {
        var apiKey   = _config["SendGrid:ApiKey"]   ?? string.Empty;
        var fromEmail = _config["SendGrid:FromEmail"] ?? "noreply@suneer.dev";
        var fromName  = _config["SendGrid:FromName"]  ?? "Suneer Portfolio";

        // Dev fallback: no API key configured — log the code so development works without email
        if (string.IsNullOrWhiteSpace(apiKey))
        {
            _logger.LogWarning(
                "[DEV] SendGrid API key not set. Password reset code for {Email}: {Code}",
                toEmail, code);
            return;
        }

        var client  = new SendGridClient(apiKey);
        var from    = new EmailAddress(fromEmail, fromName);
        var to      = new EmailAddress(toEmail);
        var subject = "Your Admin Password Reset Code";

        var plain = $"Your password reset code is: {code}\n\nThis code expires in 15 minutes.\n\nIf you did not request this, ignore this email.";
        var html  = BuildHtml(code);

        var msg      = MailHelper.CreateSingleEmail(from, to, subject, plain, html);
        var response = await client.SendEmailAsync(msg);

        if (!response.IsSuccessStatusCode)
        {
            // Log but do not throw — we never reveal to the user whether email delivery succeeded
            _logger.LogWarning(
                "SendGrid delivery failed for {Email} — status {StatusCode}",
                toEmail, (int)response.StatusCode);
        }
    }

    private static string BuildHtml(string code) => $"""
        <!DOCTYPE html>
        <html lang="en">
        <head>
          <meta charset="UTF-8" />
          <meta name="viewport" content="width=device-width,initial-scale=1"/>
          <title>Password Reset</title>
        </head>
        <body style="margin:0;padding:0;background:#0f172a;font-family:'Segoe UI',sans-serif">
          <table width="100%" cellpadding="0" cellspacing="0">
            <tr><td align="center" style="padding:40px 16px">
              <table width="100%" style="max-width:480px;background:#1e293b;border-radius:12px;overflow:hidden">

                <!-- Header -->
                <tr>
                  <td style="background:linear-gradient(135deg,#1e3a5f,#0f2238);padding:32px 40px;text-align:center">
                    <p style="margin:0;font-size:1.4rem;font-weight:800;color:#fff;letter-spacing:-.5px">
                      <span style="color:#3b82f6">&lt;</span>SR<span style="color:#3b82f6">/&gt;</span>
                    </p>
                    <p style="margin:8px 0 0;color:#94a3b8;font-size:.85rem">Admin Password Reset</p>
                  </td>
                </tr>

                <!-- Body -->
                <tr>
                  <td style="padding:40px">
                    <p style="margin:0 0 16px;color:#e2e8f0;font-size:.95rem;line-height:1.6">
                      A password reset was requested for your admin account.
                      Use the code below to proceed. It expires in <strong style="color:#60a5fa">15 minutes</strong>.
                    </p>

                    <!-- Code box -->
                    <div style="background:#0f172a;border:1px solid #334155;border-radius:10px;
                                text-align:center;padding:28px;margin:28px 0">
                      <p style="margin:0 0 8px;color:#64748b;font-size:.8rem;
                                 letter-spacing:.12em;text-transform:uppercase">Reset Code</p>
                      <p style="margin:0;font-size:2.8rem;font-weight:900;letter-spacing:.25em;
                                 color:#60a5fa;font-variant-numeric:tabular-nums">{code}</p>
                    </div>

                    <p style="margin:0 0 16px;color:#94a3b8;font-size:.85rem;line-height:1.6">
                      Enter this code on the verification page. If you did not request a
                      password reset, you can safely ignore this email — your password
                      will remain unchanged.
                    </p>

                    <div style="background:#0f172a;border-left:3px solid #f59e0b;
                                border-radius:0 6px 6px 0;padding:12px 16px">
                      <p style="margin:0;color:#fbbf24;font-size:.8rem">
                        <strong>Security note:</strong> Never share this code with anyone.
                        Suneer Portfolio staff will never ask for your reset code.
                      </p>
                    </div>
                  </td>
                </tr>

                <!-- Footer -->
                <tr>
                  <td style="padding:16px 40px 32px;text-align:center">
                    <p style="margin:0;color:#475569;font-size:.75rem">
                      &copy; {DateTime.UtcNow.Year} Suneer Ranjitkar &mdash; suneer.dev<br/>
                      This is an automated message, please do not reply.
                    </p>
                  </td>
                </tr>

              </table>
            </td></tr>
          </table>
        </body>
        </html>
        """;
}
