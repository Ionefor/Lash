using CSharpFunctionalExtensions;
using ErrorsFlow.Errors;
using ErrorsFlow.Models;
using Lash.Users.Application.Abstractions;
using Lash.Users.Application.Constants;
using Lash.Users.Application.Models;
using Microsoft.Extensions.Logging;
using WebFlow.Abstractions.Interfaces;

namespace Lash.Users.Application.Features.Commands.Refresh;

public sealed class RefreshTokensHandler(
    IRefreshSessionManager refreshSessionManager,
    IUserAccountService accounts,
    ITokenProvider tokenProvider,
    IUnitOfWork unitOfWork,
    IUserSessionLock userSessionLock,
    ILogger<RefreshTokensHandler> logger,
    TimeProvider timeProvider) : ICommandHandler<RefreshTokensCommand, AuthTokens>
{
    public async Task<Result<AuthTokens, ErrorList>> Handle(RefreshTokensCommand command, CancellationToken cancellationToken = default)
    {
        var session = await refreshSessionManager.GetByRefreshTokenAsync(command.RefreshToken, cancellationToken);
        if (session.IsFailure)
        {
            logger.LogWarning("Token refresh failed because refresh session was invalid.");
            return session.Error.ToErrorList();
        }

        var claims = await tokenProvider.GetClaimsFromExpiredAccessTokenAsync(command.AccessToken, cancellationToken);
        if (claims.IsFailure)
        {
            logger.LogWarning("Token refresh failed because access token was invalid.");
            return claims.Error.ToErrorList();
        }

        var userId = claims.Value.FirstOrDefault(claim => claim.Type == AccessTokenClaimTypes.Sub)?.Value;
        var jti = claims.Value.FirstOrDefault(claim => claim.Type == AccessTokenClaimTypes.Jti)?.Value;
        if (!Guid.TryParse(userId, out var parsedUserId) || !Guid.TryParse(jti, out var parsedJti) ||
            parsedUserId != session.Value.UserId)
        {
            logger.LogWarning("Token refresh rejected because token claims did not match the refresh session.");
            return AuthErrors.TokenInvalid().ToErrorList();
        }

        await using var transaction = await unitOfWork.BeginTransactionAsync(cancellationToken);
        if (!await userSessionLock.TryAcquireAsync(session.Value.UserId, cancellationToken))
        {
            return AuthErrors.TokenInvalid().ToErrorList();
        }

        session = await refreshSessionManager.GetByRefreshTokenAsync(command.RefreshToken, cancellationToken);
        if (session.IsFailure || parsedJti != session.Value.Jti)
        {
            return AuthErrors.RefreshTokenInvalid().ToErrorList();
        }

        var user = await accounts.FindByIdAsync(session.Value.UserId, cancellationToken);
        if (user is null)
        {
            logger.LogWarning("Token refresh was requested for a missing user {UserId}.", session.Value.UserId);
            return AuthErrors.TokenInvalid().ToErrorList();
        }

        if (!await refreshSessionManager.TryRevokeAsync(session.Value.Id, timeProvider.GetUtcNow(), cancellationToken))
        {
            logger.LogWarning("Token refresh failed because refresh session was already revoked.");
            return AuthErrors.RefreshTokenInvalid().ToErrorList();
        }

        var accessToken = await tokenProvider.GenerateAccessTokenAsync(user, cancellationToken);
        var refreshToken = await tokenProvider.GenerateRefreshTokenAsync(
            user,
            accessToken.Jti,
            session.Value.AbsoluteExpiresAt,
            cancellationToken);
        if (refreshToken.IsFailure)
        {
            logger.LogWarning("Token refresh could not create a replacement refresh session for user {UserId}.", user.Id);
            return refreshToken.Error.ToErrorList();
        }

        await transaction.CommitAsync(cancellationToken);
        logger.LogInformation("Token refresh completed for user {UserId}.", user.Id);
        return new AuthTokens(accessToken.AccessToken, refreshToken.Value);
    }
}
