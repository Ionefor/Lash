using ErrorsFlow.Errors;
using Lash.Users.Domain;

namespace Lash.Users.UnitTests.Domain;

public sealed class RefreshSessionTests
{
    private static readonly DateTimeOffset CreatedAt = new(2026, 9, 27, 0, 0, 0, TimeSpan.Zero);

    [Fact]
    public void Create_WhenExpirationIsNotInFuture_ReturnsInvalidValueError()
    {
        var result = RefreshSession.Create(Guid.NewGuid(), Guid.NewGuid(), "token-hash", CreatedAt, CreatedAt);

        Assert.True(result.IsFailure);
        Assert.Equal(GeneralErrorCodes.ValueIsInvalid, result.Error.Code);
        Assert.Equal("expiresAt", result.Error.Target);
    }

    [Fact]
    public void Revoke_WhenCalledTwice_KeepsOriginalRevocationTime()
    {
        var session = CreateSession();
        var revokedAt = session.CreatedAt.AddMinutes(1);

        var firstResult = session.Revoke(revokedAt);
        var secondResult = session.Revoke(session.CreatedAt.AddTicks(-1));

        Assert.True(firstResult.IsSuccess);
        Assert.True(secondResult.IsSuccess);
        Assert.True(session.IsRevoked);
        Assert.Equal(revokedAt, session.RevokedAt);
    }

    [Fact]
    public void EnsureActive_WhenSessionIsExpired_ReturnsRefreshTokenExpiredError()
    {
        var session = CreateSession();

        var result = session.EnsureActive(session.ExpiresAt);

        Assert.True(result.IsFailure);
        Assert.Equal(AuthErrorCodes.RefreshTokenExpired, result.Error.Code);
    }

    [Fact]
    public void EnsureActive_WhenAbsoluteLifetimeIsExpired_ReturnsRefreshTokenExpiredError()
    {
        var result = RefreshSession.Create(
            Guid.NewGuid(),
            Guid.NewGuid(),
            "token-hash",
            CreatedAt,
            CreatedAt.AddHours(12),
            CreatedAt.AddDays(1));

        Assert.True(result.IsSuccess);

        var activeResult = result.Value.EnsureActive(result.Value.AbsoluteExpiresAt);

        Assert.True(activeResult.IsFailure);
        Assert.Equal(AuthErrorCodes.RefreshTokenExpired, activeResult.Error.Code);
    }

    [Fact]
    public void EnsureActive_WhenSessionIsRevoked_ReturnsRefreshTokenInvalidError()
    {
        var session = CreateSession();
        session.Revoke(session.CreatedAt);

        var result = session.EnsureActive(session.CreatedAt);

        Assert.True(result.IsFailure);
        Assert.Equal(AuthErrorCodes.RefreshTokenInvalid, result.Error.Code);
    }

    private static RefreshSession CreateSession()
    {
        var result = RefreshSession.Create(
            Guid.NewGuid(),
            Guid.NewGuid(),
            "token-hash",
            CreatedAt,
            CreatedAt.AddDays(30));

        Assert.True(result.IsSuccess);
        Assert.Equal(CreatedAt, result.Value.CreatedAt);
        return result.Value;
    }
}
