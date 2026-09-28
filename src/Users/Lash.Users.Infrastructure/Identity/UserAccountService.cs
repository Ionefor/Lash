using CSharpFunctionalExtensions;
using ErrorsFlow.Errors;
using ErrorsFlow.Models;
using Lash.Users.Application.Abstractions;
using Lash.Users.Application.Errors;
using Lash.Users.Application.Models;
using Lash.Users.Infrastructure.DbContexts;
using Microsoft.AspNetCore.Identity;

namespace Lash.Users.Infrastructure.Identity;

public sealed class UserAccountService(
    UserManager<IdentityUserEntity> userManager,
    RoleManager<IdentityRoleEntity> roleManager,
    UsersDbContext dbContext) : IUserAccountService
{
    public async Task<bool> RoleExistsAsync(string name, CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        return await roleManager.RoleExistsAsync(name);
    }

    public async Task<UserAccount?> FindByEmailAsync(string email, CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        return (await userManager.FindByEmailAsync(email)) is { } user ? Map(user) : null;
    }

    public async Task<UserAccount?> FindByIdAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        return (await userManager.FindByIdAsync(userId.ToString())) is { } user ? Map(user) : null;
    }

    public async Task<IReadOnlyList<string>> GetRolesAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        var user = await userManager.FindByIdAsync(userId.ToString());
        return user is null ? [] : (await userManager.GetRolesAsync(user)).ToArray();
    }

    public async Task<Result<UserAccount, Error>> CreateAsync(string email, string password, string role, CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        var ownsTransaction = dbContext.Database.CurrentTransaction is null;
        await using var transaction = ownsTransaction
            ? await dbContext.Database.BeginTransactionAsync(cancellationToken)
            : null;

        if (!await roleManager.RoleExistsAsync(role))
            return Result.Failure<UserAccount, Error>(GeneralErrors.Failed("Required role is not configured."));

        var user = IdentityUserEntity.Create(email);
        var result = await userManager.CreateAsync(user, password);
        if (!result.Succeeded) return Result.Failure<UserAccount, Error>(Map(result));

        var addToRole = await userManager.AddToRoleAsync(user, role);
        if (!addToRole.Succeeded) return Result.Failure<UserAccount, Error>(Map(addToRole));

        if (transaction is not null)
        {
            await transaction.CommitAsync(cancellationToken);
        }

        return Map(user);
    }

    public async Task<Result<UserAccount, Error>> AuthenticateAsync(string email, string password, CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        var user = await userManager.FindByEmailAsync(email);
        if (user is null || await userManager.IsLockedOutAsync(user)) return Result.Failure<UserAccount, Error>(AuthErrors.CredentialsInvalid());
        if (!await userManager.CheckPasswordAsync(user, password))
        {
            var failed = await userManager.AccessFailedAsync(user);
            return failed.Succeeded ? Result.Failure<UserAccount, Error>(AuthErrors.CredentialsInvalid()) : Result.Failure<UserAccount, Error>(GeneralErrors.Failed("Unable to process login."));
        }
        var reset = await userManager.ResetAccessFailedCountAsync(user);
        return reset.Succeeded ? Map(user) : Result.Failure<UserAccount, Error>(GeneralErrors.Failed("Unable to process login."));
    }

    public async Task<UnitResult<Error>> ConfirmEmailAsync(Guid userId, string code, CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        var user = await userManager.FindByIdAsync(userId.ToString());
        if (user is null) return UnitResult.Failure(UsersApplicationErrors.EmailConfirmationCodeInvalid());

        var result = await userManager.ConfirmEmailAsync(user, code);
        if (result.Succeeded) return UnitResult.Success<Error>();

        return result.Errors.Any(error => error.Code == nameof(IdentityErrorDescriber.InvalidToken))
            ? UnitResult.Failure(UsersApplicationErrors.EmailConfirmationCodeInvalid())
            : UnitResult.Failure(Map(result));
    }

    public async Task<UnitResult<Error>> ResetPasswordAsync(Guid userId, string code, string password, CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        var user = await userManager.FindByIdAsync(userId.ToString());
        if (user is null) return UnitResult.Failure(UsersApplicationErrors.PasswordResetCodeInvalid());
        var result = await userManager.ResetPasswordAsync(user, code, password);
        if (result.Succeeded) return UnitResult.Success<Error>();
        return result.Errors.Any(error => error.Code == nameof(IdentityErrorDescriber.InvalidToken))
            ? UnitResult.Failure(UsersApplicationErrors.PasswordResetCodeInvalid()) : UnitResult.Failure(Map(result));
    }

    public async Task<UnitResult<Error>> ChangePasswordAsync(Guid userId, string currentPassword, string password, CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        var user = await userManager.FindByIdAsync(userId.ToString());
        if (user is null) return UnitResult.Failure(AuthErrors.CredentialsInvalid());
        if (!await userManager.CheckPasswordAsync(user, currentPassword)) return UnitResult.Failure(AuthErrors.CredentialsInvalid());
        var result = await userManager.ChangePasswordAsync(user, currentPassword, password);
        return result.Succeeded ? UnitResult.Success<Error>() : UnitResult.Failure(Map(result));
    }

    private static UserAccount Map(IdentityUserEntity user) => new(user.Id, user.Email, user.EmailConfirmed);

    private static Error Map(IdentityResult result)
    {
        var error = result.Errors.FirstOrDefault();
        return error?.Code switch
        {
            nameof(IdentityErrorDescriber.DuplicateUserName) or nameof(IdentityErrorDescriber.DuplicateEmail) => GeneralErrors.ValueAlreadyExists("email"),
            nameof(IdentityErrorDescriber.InvalidEmail) or nameof(IdentityErrorDescriber.InvalidUserName) => GeneralErrors.ValueIsInvalid("email"),
            nameof(IdentityErrorDescriber.PasswordTooShort) or nameof(IdentityErrorDescriber.PasswordRequiresNonAlphanumeric) or nameof(IdentityErrorDescriber.PasswordRequiresDigit) or nameof(IdentityErrorDescriber.PasswordRequiresLower) or nameof(IdentityErrorDescriber.PasswordRequiresUpper) or nameof(IdentityErrorDescriber.PasswordRequiresUniqueChars) => GeneralErrors.ValueIsInvalid("password"),
            _ => GeneralErrors.Failed("Unable to process user account.")
        };
    }
}
