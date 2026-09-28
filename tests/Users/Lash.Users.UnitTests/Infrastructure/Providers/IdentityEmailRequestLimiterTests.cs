using Lash.Users.Application.Abstractions;
using Lash.Users.Infrastructure.Options;
using Lash.Users.Infrastructure.Providers;
using Microsoft.Extensions.Options;
using Moq;

namespace Lash.Users.UnitTests.Infrastructure.Providers;

public sealed class IdentityEmailRequestLimiterTests
{
    [Fact]
    public async Task TryAcquireAsync_WhenEmailIsEmpty_ReturnsFalseWithoutAccessingDatabase()
    {
        var store = new Mock<IIdentityEmailRequestStore>();
        var limiter = CreateLimiter(store.Object);

        var acquired = await limiter.TryAcquireAsync(" ", IdentityEmailOperation.PasswordReset);

        Assert.False(acquired);
        store.Verify(item => item.TryAcquireAsync(
            It.IsAny<string>(),
            It.IsAny<string>(),
            It.IsAny<DateTimeOffset>(),
            It.IsAny<int>(),
            It.IsAny<TimeSpan>(),
            It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task TryAcquireAsync_WhenOperationIsUnknown_ThrowsBeforeAccessingDatabase()
    {
        var store = new Mock<IIdentityEmailRequestStore>();
        var limiter = CreateLimiter(store.Object);

        await Assert.ThrowsAsync<ArgumentOutOfRangeException>(() =>
            limiter.TryAcquireAsync("user@example.com", (IdentityEmailOperation)999));

        store.Verify(item => item.TryAcquireAsync(
            It.IsAny<string>(),
            It.IsAny<string>(),
            It.IsAny<DateTimeOffset>(),
            It.IsAny<int>(),
            It.IsAny<TimeSpan>(),
            It.IsAny<CancellationToken>()), Times.Never);
    }

    [Theory]
    [InlineData(IdentityEmailOperation.EmailConfirmation, 2, 5, "EmailConfirmation")]
    [InlineData(IdentityEmailOperation.PasswordReset, 4, 20, "PasswordReset")]
    public async Task TryAcquireAsync_WhenEmailIsValid_UsesConfiguredRule(
        IdentityEmailOperation operation,
        int expectedLimit,
        int expectedWindowMinutes,
        string expectedOperation)
    {
        var store = new Mock<IIdentityEmailRequestStore>();
        store.Setup(item => item.TryAcquireAsync(
                It.IsAny<string>(),
                expectedOperation,
                It.IsAny<DateTimeOffset>(),
                expectedLimit,
                TimeSpan.FromMinutes(expectedWindowMinutes),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);
        var limiter = CreateLimiter(store.Object, new IdentityEmailRateLimitOptions
        {
            EmailConfirmationLimit = 2,
            EmailConfirmationWindowMinutes = 5,
            PasswordResetLimit = 4,
            PasswordResetWindowMinutes = 20
        });

        var acquired = await limiter.TryAcquireAsync("user@example.com", operation);

        Assert.True(acquired);
        store.Verify(item => item.TryAcquireAsync(
            It.IsAny<string>(),
            expectedOperation,
            It.IsAny<DateTimeOffset>(),
            expectedLimit,
            TimeSpan.FromMinutes(expectedWindowMinutes),
            It.IsAny<CancellationToken>()), Times.Once);
    }

    private static IdentityEmailRequestLimiter CreateLimiter(
        IIdentityEmailRequestStore store,
        IdentityEmailRateLimitOptions? options = null) =>
        new(Options.Create(options ?? new IdentityEmailRateLimitOptions()), TimeProvider.System, store);
}
