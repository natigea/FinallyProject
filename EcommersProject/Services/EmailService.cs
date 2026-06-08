using SendGrid;
using SendGrid.Helpers.Mail;

namespace EcommersProject.Services;

public class EmailService(IConfiguration config, ILogger<EmailService> logger)
{
    public async Task SendVerificationCodeAsync(string toEmail, string code)
    {
        var subject = "Alıs-Veris — Kod təsdiqləmə";
        var body = $@"
<div style=""font-family:'Inter',Arial,sans-serif;max-width:480px;margin:0 auto;padding:32px;background:#fff;border-radius:16px;border:1px solid #e5e7eb;"">
    <div style=""text-align:center;margin-bottom:24px;"">
        <h2 style=""margin:0;font-size:1.3rem;color:#1a1a1a;"">Alıs-Veris</h2>
        <p style=""margin:6px 0 0;color:#888;font-size:.85rem;"">Təsdiqləmə kodu</p>
    </div>
    <div style=""text-align:center;margin:28px 0;"">
        <div style=""display:inline-block;padding:16px 40px;background:linear-gradient(135deg,#2563eb,#6366f1);border-radius:12px;letter-spacing:8px;font-size:2rem;font-weight:800;color:#fff;"">{code}</div>
    </div>
    <p style=""text-align:center;color:#666;font-size:.85rem;line-height:1.6;"">
        Bu kodu daxil edin. Kod <b>10 dəqiqə</b> ərzində etibarlıdır.
    </p>
    <hr style=""border:none;border-top:1px solid #eee;margin:24px 0;"" />
    <p style=""text-align:center;color:#aaa;font-size:.75rem;"">Bu e-poçtu siz tələb etməmisinizsə, nəzərə almayın.</p>
</div>";
        await SendAsync(toEmail, subject, body);
    }

    private async Task SendAsync(string toEmail, string subject, string htmlBody)
    {
        var apiKey = config["SendGrid:ApiKey"]!;
        var fromEmail = config["SendGrid:FromEmail"]!;
        var fromName = config["SendGrid:FromName"] ?? "Alıs-Veris";

        var client = new SendGridClient(apiKey);
        var from = new EmailAddress(fromEmail, fromName);
        var to = new EmailAddress(toEmail);
        var msg = MailHelper.CreateSingleEmail(from, to, subject, null, htmlBody);

        try
        {
            var response = await client.SendEmailAsync(msg);
            logger.LogInformation("[Email] SendGrid response {Status} to {To}", response.StatusCode, toEmail);

            if ((int)response.StatusCode >= 400)
            {
                var body = await response.Body.ReadAsStringAsync();
                logger.LogError("[Email] SendGrid error: {Body}", body);
                throw new Exception($"SendGrid returned {response.StatusCode}: {body}");
            }
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "[Email] Failed to send to {To}", toEmail);
            throw;
        }
    }
}
