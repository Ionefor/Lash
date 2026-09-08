using ErrorsFlow.Errors;
using Lash.Users.Domain;

namespace Lash.Users.UnitTests.Domain;

public sealed class RefreshSessionTests
{
    [Fact]
    public void Create_WhenExpirationIsNotInFuture_ReturnsInvalidValueError()
    {
        var result = RefreshSession.Create(Guid.NewGuid(), Guid.NewGuid(), "token-hash", DateTimeOffset.MinValue);

        Assert.True(result.IsFailure);
        Assert.Equal(GeneralErrorCodes.ValueIsInvalid, result.Error.Code);
        Assert.Equal(nameof(RefreshSession.ExpiresAt), result.Error.Target);
    }

    [Fact]
    public void Revoke_WhenCalledTwice_KeepsOriginalRevocationTime()
    {
        var session = CreateSession();
        var revokedAt = session.CreatedAt.AddMinutes(1);

        var firstResult = session.Revoke(revokedAt);
        var secondResult = session.Revoke(revokedAt.AddMinutes(1));

        Assert.True(firstResult.IsSuccess);
        Assert.True(secondResult.IsSuccess);
        Assert.True(session.IsRevoked);
        Assert.Equal(revokedAt, session.RevokedAt);
    }

    private static RefreshSession CreateSession()
    {
        var result = RefreshSession.Create(Guid.NewGuid(), Guid.NewGuid(), "token-hash", DateTimeOffset.MaxValue);

        Assert.True(result.IsSuccess);
        return result.Value;
    }
}
