using System.Net;
using System.Net.Mail;
using System.Net.Mime;
using System.Text;
using Lash.Users.Infrastructure.Options;
using Microsoft.Extensions.Options;

namespace Lash.Users.Infrastructure.Providers;

public sealed class SmtpEmailTransport(IOptions<EmailOptions> options) : IEmailTransport
{
    public async Task SendAsync(EmailMessage message, CancellationToken cancellationToken = default)
    {
        var emailOptions = options.Value;
        using var client = new SmtpClient(emailOptions.Host, emailOptions.Port)
        {
            EnableSsl = emailOptions.UseSsl,
            Credentials = new NetworkCredential(emailOptions.UserName, emailOptions.Password)
        };
        using var mailMessage = new MailMessage(
            emailOptions.FromAddress,
            message.To,
            message.Subject,
            message.PlainTextBody)
        {
            SubjectEncoding = Encoding.UTF8,
            BodyEncoding = Encoding.UTF8
        };
        if (message.EventId is { } eventId)
        {
            mailMessage.Headers["Message-ID"] = $"<{eventId:N}@lash.local>";
        }
        mailMessage.AlternateViews.Add(AlternateView.CreateAlternateViewFromString(
            message.HtmlBody,
            Encoding.UTF8,
            MediaTypeNames.Text.Html));

        await client.SendMailAsync(mailMessage, cancellationToken);
    }
}
