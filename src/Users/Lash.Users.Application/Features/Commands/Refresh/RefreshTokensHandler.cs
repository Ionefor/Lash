using CSharpFunctionalExtensions;
using ErrorsFlow.Errors;
using ErrorsFlow.Models;
using Lash.Users.Application.Abstractions;
using Lash.Users.Application.Models;
using WebFlow.Abstractions.Interfaces;

namespace Lash.Users.Application.Features.Commands.Refresh;

public sealed class RefreshTokensHandler(
    IRefreshSessionManager refreshSessionManager,
    ITokenProvider tokenProvider,
    IUnitOfWork unitOfWork) : ICommandHandler<RefreshTokensCommand, AuthTokens>
{
    public async Task<Result<AuthTokens, ErrorList>> Handle(RefreshTokensCommand command, CancellationToken cancellationToken = default)
    {
        var session = await refreshSessionManager.GetByRefreshTokenAsync(command.RefreshToken, cancellationToken);
        if (session.IsFailure)
            return session.Error.ToErrorList();

        var claims = await tokenProvider.GetClaimsFromExpiredAccessTokenAsync(command.AccessToken, cancellationToken);
        if (claims.IsFailure)
            return claims.Error.ToErrorList();

        var userId = claims.Value.FirstOrDefault(claim => claim.Type == AccessTokenClaimTypes.Subject)?.Value;
        var jti = claims.Value.FirstOrDefault(claim => claim.Type == AccessTokenClaimTypes.JwtId)?.Value;
        if (!Guid.TryParse(userId, out var parsedUserId) || !Guid.TryParse(jti, out var parsedJti) ||
            parsedUserId != session.Value.UserId || parsedJti != session.Value.Jti)
            return AuthErrors.TokenInvalid().ToErrorList();

        var revoke = session.Value.Revoke(DateTimeOffset.UtcNow);
        if (revoke.IsFailure)
            return revoke.Error.ToErrorList();

        var accessToken = await tokenProvider.GenerateAccessTokenAsync(session.Value.User, cancellationToken);
        var refreshToken = await tokenProvider.GenerateRefreshTokenAsync(session.Value.User, accessToken.Jti, cancellationToken);
        if (refreshToken.IsFailure)
            return refreshToken.Error.ToErrorList();

        await unitOfWork.SaveChangesAsync(cancellationToken);
        return new AuthTokens(accessToken.AccessToken, refreshToken.Value);
    }
}
