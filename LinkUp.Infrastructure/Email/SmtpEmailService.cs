using LinkUp.Shared.Emails;
using MailKit.Net.Smtp;
using MailKit.Security;
using MimeKit;
using Microsoft.Extensions.Options;

namespace LinkUp.Infrastructure.Email;

public class SmtpEmailService : IEmailSender
{
    private readonly EmailSenderOptions _options;

    public SmtpEmailService(IOptions<EmailSenderOptions> options)
    {
        _options = options.Value;
    }

    public async Task SendEmailAsync(string to, string subject, string body)
    {
        var message = new MimeMessage();
        message.From.Add(new MailboxAddress(_options.FromName, _options.FromEmail));
        message.To.Add(MailboxAddress.Parse(to));
        message.Subject = subject;
        message.Body = new TextPart("html") { Text = body };

        // Puerto 465 → SSL directo | Puerto 587 (y otros) → STARTTLS
        var socketOptions = _options.Port == 465
            ? SecureSocketOptions.SslOnConnect
            : SecureSocketOptions.StartTls;

        using var client = new SmtpClient();
        await client.ConnectAsync(_options.Host, _options.Port, socketOptions);
        await client.AuthenticateAsync(_options.Username, _options.Password);
        await client.SendAsync(message);
        await client.DisconnectAsync(true);
    }
}
