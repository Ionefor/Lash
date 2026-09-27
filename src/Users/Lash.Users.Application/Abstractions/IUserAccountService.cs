using CSharpFunctionalExtensions;
using ErrorsFlow.Models;
using Lash.Users.Application.Models;

namespace Lash.Users.Application.Abstractions;

public interface IUserAccountService
{
    Task<bool> RoleExistsAsync(string name, CancellationToken cancellationToken = default);
    Task<Result<UserAccount, Error>> CreateAsync(string email, string password, string role, CancellationToken cancellationToken = default);
    Task<UserAccount?> FindByEmailAsync(string email, CancellationToken cancellationToken = default);
    Task<UserAccount?> FindByIdAsync(Guid userId, CancellationToken cancellationToken = default);
    Task<Result<UserAccount, Error>> AuthenticateAsync(string email, string password, CancellationToken cancellationToken = default);
    Task<UnitResult<Error>> ConfirmEmailAsync(Guid userId, string code, CancellationToken cancellationToken = default);
    Task<UnitResult<Error>> ResetPasswordAsync(Guid userId, string code, string password, CancellationToken cancellationToken = default);
    Task<UnitResult<Error>> ChangePasswordAsync(Guid userId, string currentPassword, string password, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<string>> GetRolesAsync(Guid userId, CancellationToken cancellationToken = default);
}
