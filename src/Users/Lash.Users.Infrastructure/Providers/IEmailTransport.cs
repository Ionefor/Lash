namespace Lash.Users.Infrastructure.Providers;

public interface IEmailTransport
{
    Task SendAsync(EmailMessage message, CancellationToken cancellationToken = default);
}
