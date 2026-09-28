namespace Lash.Users.Infrastructure.Providers;

public sealed record EmailMessage(
    string To,
    string Subject,
    string PlainTextBody,
    string HtmlBody,
    Guid? EventId = null);
