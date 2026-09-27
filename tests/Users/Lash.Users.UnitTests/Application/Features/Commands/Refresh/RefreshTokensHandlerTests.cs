using System.Security.Claims;
using CSharpFunctionalExtensions;
using ErrorsFlow.Errors;
using ErrorsFlow.Models;
using Lash.Users.Application.Abstractions;
using Lash.Users.Application.Constants;
using Lash.Users.Application.Features.Commands.Refresh;
using Lash.Users.Application.Models;
using Lash.Users.Domain;
using Moq;
using Microsoft.Extensions.Logging.Abstractions;
using WebFlow.Abstractions.Interfaces;

namespace Lash.Users.UnitTests.Application.Features.Commands.Refresh;

public sealed class RefreshTokensHandlerTests
{
    [Fact]
    public async Task Handle_WhenSessionIsRevokedByAnotherRequest_ReturnsRefreshTokenInvalid()
    {
        var session = CreateSession();
        var refreshSessions = new Mock<IRefreshSessionManager>();
        refreshSessions.Setup(item => item.GetByRefreshTokenAsync("refresh", It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result.Success<RefreshSession, Error>(session));
        refreshSessions.Setup(item => item.TryRevokeAsync(session.Id, It.IsAny<DateTimeOffset>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);
        var accounts = CreateAccounts(session.UserId);
        var tokens = CreateTokens(session);
        var handler = new RefreshTokensHandler(refreshSessions.Object, accounts.Object, tokens.Object, CreateUnitOfWork().Object, NullLogger<RefreshTokensHandler>.Instance);

        var result = await handler.Handle(new RefreshTokensCommand("access", "refresh"));

        Assert.True(result.IsFailure);
        Assert.Equal(AuthErrorCodes.RefreshTokenInvalid, result.Error[0].Code);
        tokens.Verify(item => item.GenerateRefreshTokenAsync(It.IsAny<UserAccount>(), It.IsAny<Guid>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task Handle_WhenSessionIsActive_RevokesItBeforeCreatingReplacementTokens()
    {
        var session = CreateSession();
        var refreshSessions = new Mock<IRefreshSessionManager>();
        refreshSessions.Setup(item => item.GetByRefreshTokenAsync("refresh", It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result.Success<RefreshSession, Error>(session));
        refreshSessions.Setup(item => item.TryRevokeAsync(session.Id, It.IsAny<DateTimeOffset>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);
        var accounts = CreateAccounts(session.UserId);
        var tokens = CreateTokens(session);
        var unitOfWork = CreateUnitOfWork();
        var handler = new RefreshTokensHandler(refreshSessions.Object, accounts.Object, tokens.Object, unitOfWork.Object, NullLogger<RefreshTokensHandler>.Instance);

        var result = await handler.Handle(new RefreshTokensCommand("access", "refresh"));

        Assert.True(result.IsSuccess);
        Assert.Equal(new AuthTokens("new-access", "new-refresh"), result.Value);
        refreshSessions.Verify(item => item.TryRevokeAsync(session.Id, It.IsAny<DateTimeOffset>(), It.IsAny<CancellationToken>()), Times.Once);
        unitOfWork.Verify(item => item.BeginTransactionAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    private static RefreshSession CreateSession()
    {
        var result = RefreshSession.Create(
            Guid.NewGuid(),
            Guid.NewGuid(),
            "hash",
            new DateTimeOffset(2026, 9, 27, 0, 0, 0, TimeSpan.Zero),
            new DateTimeOffset(2026, 10, 27, 0, 0, 0, TimeSpan.Zero));
        Assert.True(result.IsSuccess);
        return result.Value;
    }

    private static Mock<IUserAccountService> CreateAccounts(Guid userId)
    {
        var accounts = new Mock<IUserAccountService>();
        accounts.Setup(item => item.FindByIdAsync(userId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new UserAccount(userId, "user@example.com", true));
        return accounts;
    }

    private static Mock<ITokenProvider> CreateTokens(RefreshSession session)
    {
        var tokens = new Mock<ITokenProvider>();
        tokens.Setup(item => item.GetClaimsFromExpiredAccessTokenAsync("access", It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result.Success<IReadOnlyList<Claim>, Error>([
                new Claim(AccessTokenClaimTypes.Sub, session.UserId.ToString()),
                new Claim(AccessTokenClaimTypes.Jti, session.Jti.ToString())
            ]));
        tokens.Setup(item => item.GenerateAccessTokenAsync(It.IsAny<UserAccount>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new JwtTokenResult("new-access", Guid.NewGuid()));
        tokens.Setup(item => item.GenerateRefreshTokenAsync(It.IsAny<UserAccount>(), It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result.Success<string, Error>("new-refresh"));
        return tokens;
    }

    private static Mock<IUnitOfWork> CreateUnitOfWork()
    {
        var transaction = new Mock<ITransaction>();
        var unitOfWork = new Mock<IUnitOfWork>();
        unitOfWork.Setup(item => item.BeginTransactionAsync(It.IsAny<CancellationToken>())).ReturnsAsync(transaction.Object);
        return unitOfWork;
    }
}
