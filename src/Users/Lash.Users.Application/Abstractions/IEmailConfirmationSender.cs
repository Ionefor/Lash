using CSharpFunctionalExtensions;
using ErrorsFlow.Models;
using Lash.Users.Domain;

namespace Lash.Users.Application.Abstractions;

public interface IEmailConfirmationSender
{
    Task<UnitResult<Error>> SendAsync(User user, CancellationToken cancellationToken = default);
}

public interface IPasswordResetSender
{
    Task<UnitResult<Error>> SendAsync(User user, CancellationToken cancellationToken = default);
}

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
