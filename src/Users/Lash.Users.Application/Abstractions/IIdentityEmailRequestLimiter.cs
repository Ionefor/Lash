namespace Lash.Users.Application.Abstractions;

public enum IdentityEmailOperation
{
    EmailConfirmation,
    PasswordReset
}

public interface IIdentityEmailRequestLimiter
{
    Task<bool> TryAcquireAsync(
        string email,
        IdentityEmailOperation operation,
        CancellationToken cancellationToken = default);
}
