using System.Text.Json;
using Microsoft.Extensions.Options;
using SendGrid;
using SendGrid.Helpers.Mail;

namespace Application.Services.Mail;

public class EmailService : IEmailService
{
    private readonly IOptions<MailSettingsOptions> _mailOptions;

    public EmailService(IOptions<MailSettingsOptions> mailOptions)
    {
        _mailOptions = mailOptions;
    }

    public async Task<bool> SendEmail(EmailDto notification)
    {
        var client = new SendGridClient(_mailOptions.Value.Password);
        var from = new EmailAddress(_mailOptions.Value.From, _mailOptions.Value.MailBoxText ?? "REMAX Türkiye");
        var subject = notification.Subject;
        var to = new EmailAddress(notification.To[0]);
        var plainTextContent = ""; // SendGrid plain text content gerektirir
        var htmlContent = notification.Body;
        var msg = MailHelper.CreateSingleEmail(from, to, subject, plainTextContent, htmlContent);
        
        // CC ekle
        if (notification.Cc != null && notification.Cc.Any())
        {
            msg.AddCcs(notification.Cc.Select(cc => new EmailAddress(cc)).ToList());
        }

        // ReplyTo ekle
        if (notification.ReplyTo != null && notification.ReplyTo.Any())
        {
            msg.ReplyTo = new EmailAddress(notification.ReplyTo[0]);
        }

        var response = await client.SendEmailAsync(msg);
                
        return response.IsSuccessStatusCode;
    }
}

public interface IEmailService
{
    /// <summary>
    /// Send Email using SendGrid API
    /// </summary>
    /// <param name="notification">Email notification details</param>
    /// <returns>True if email was sent successfully</returns>
    Task<bool> SendEmail(EmailDto notification);
}

public class EmailDto
{
    public required string[] To { get; set; }
    public string[]? Cc { get; set; }
    public string[]? ReplyTo { get; set; }
    public required string Subject { get; set; }
    public required string Body { get; set; }
    public bool IsBodyHtml { get; set; } = false;
    public List<string> Attachments { get; set; } = new List<string>();
}

public class MailSettingsOptions
{
    public string From { get; set; } = string.Empty;
    public string Username { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
    public string Host { get; set; } = string.Empty;
    public string ReplyEmail { get; set; } = string.Empty;
    public string ReplyPassword { get; set; } = string.Empty;
    public string MailBoxText { get; set; } = string.Empty;
    public string Port { get; set; } = string.Empty;
}