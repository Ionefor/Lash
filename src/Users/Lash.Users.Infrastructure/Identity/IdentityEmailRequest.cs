namespace Lash.Users.Infrastructure.Identity;

public sealed class IdentityEmailRequest
{
    private IdentityEmailRequest()
    {
    }

    private IdentityEmailRequest(string emailHash, string operation, DateTimeOffset requestedAt)
    {
        Id = Guid.NewGuid();
        EmailHash = emailHash;
        Operation = operation;
        RequestedAt = requestedAt;
    }

    public Guid Id { get; private set; }
    public string EmailHash { get; private set; } = string.Empty;
    public string Operation { get; private set; } = string.Empty;
    public DateTimeOffset RequestedAt { get; private set; }

    public static IdentityEmailRequest Create(string emailHash, string operation, DateTimeOffset requestedAt) =>
        new(emailHash, operation, requestedAt);
}
