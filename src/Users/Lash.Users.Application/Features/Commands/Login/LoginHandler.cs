using CSharpFunctionalExtensions;
using ErrorsFlow.Errors;
using ErrorsFlow.Models;
using FluentValidation;
using Lash.Users.Application.Abstractions;
using Lash.Users.Application.Models;
using Lash.Users.Application.Errors;
using Microsoft.Extensions.Logging;
using WebFlow.Abstractions.Interfaces;
using WebFlow.FluentValidation.Extensions;

namespace Lash.Users.Application.Features.Commands.Login;

public sealed class LoginHandler(
    IValidator<LoginCommand> validator,
    IUserAccountService accounts,
    ITokenProvider tokenProvider,
    IUnitOfWork unitOfWork,
    IUserSessionLock userSessionLock,
    ILogger<LoginHandler> logger) : ICommandHandler<LoginCommand, AuthTokens>
{
    public async Task<Result<AuthTokens, ErrorList>> Handle(LoginCommand command, CancellationToken cancellationToken = default)
    {
        var validation = await validator.ValidateAsync(command, cancellationToken);
        if (!validation.IsValid)
        {
            logger.LogWarning("Login validation failed.");
            return validation.ToErrorList();
        }

        var candidateId = await accounts.FindIdByEmailAsync(command.Email, cancellationToken);
        if (candidateId is null)
        {
            _ = await accounts.AuthenticateAsync(command.Email, command.Password, cancellationToken);
            return AuthErrors.CredentialsInvalid().ToErrorList();
        }

        await using var transaction = await unitOfWork.BeginTransactionAsync(cancellationToken);
        if (!await userSessionLock.TryAcquireAsync(candidateId.Value, cancellationToken))
        {
            return AuthErrors.CredentialsInvalid().ToErrorList();
        }

        var authentication = await accounts.AuthenticateAsync(command.Email, command.Password, cancellationToken);
        if (authentication.IsFailure)
        {
            logger.LogWarning("Login failed because credentials were invalid.");
            await transaction.CommitAsync(cancellationToken);
            return authentication.Error.ToErrorList();
        }
        var user = authentication.Value;

        if (!user.EmailConfirmed)
        {
            logger.LogWarning("Login was rejected because email is not confirmed for user {UserId}.", user.Id);
            await transaction.CommitAsync(cancellationToken);
            return UsersApplicationErrors.EmailNotConfirmed().ToErrorList();
        }

        var accessToken = await tokenProvider.GenerateAccessTokenAsync(user, cancellationToken);
        var refreshToken = await tokenProvider.GenerateRefreshTokenAsync(user, accessToken.Jti, cancellationToken);
        if (refreshToken.IsFailure)
        {
            logger.LogWarning("Login could not create a refresh session for user {UserId}.", user.Id);
            return refreshToken.Error.ToErrorList();
        }

        await transaction.CommitAsync(cancellationToken);
        logger.LogInformation("Login completed for user {UserId}.", user.Id);
        return new AuthTokens(accessToken.AccessToken, refreshToken.Value);
    }
}
