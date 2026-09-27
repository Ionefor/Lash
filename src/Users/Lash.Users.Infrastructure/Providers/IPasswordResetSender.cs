using CSharpFunctionalExtensions;
using ErrorsFlow.Models;

namespace Lash.Users.Infrastructure.Providers;

public interface IPasswordResetSender
{
    Task<UnitResult<Error>> SendAsync(Guid userId, CancellationToken cancellationToken = default);
}
