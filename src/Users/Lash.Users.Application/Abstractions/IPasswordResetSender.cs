using CSharpFunctionalExtensions;
using ErrorsFlow.Models;

namespace Lash.Users.Application.Abstractions;

public interface IPasswordResetSender
{
    Task<UnitResult<Error>> SendAsync(Guid userId, CancellationToken cancellationToken = default);
}
