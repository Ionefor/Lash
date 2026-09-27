using CSharpFunctionalExtensions;
using ErrorsFlow.Errors;
using ErrorsFlow.Models;
using Lash.Users.Application.Abstractions;
using Lash.Users.Application.Features.Commands.Login;
using Lash.Users.Application.Models;
using Lash.Users.Application.Errors;
using Moq;
using Microsoft.Extensions.Logging.Abstractions;

namespace Lash.Users.UnitTests.Application.Features.Commands.Login;

public sealed class LoginHandlerTests
{
    [Fact]
    public async Task Handle_WhenEmailIsConfirmed_ReturnsAccessAndRefreshTokens()
    {
        var user = new UserAccount(Guid.NewGuid(), "user@example.com", true);
        var accounts = new Mock<IUserAccountService>();
        accounts.Setup(item => item.AuthenticateAsync("user@example.com", "Password1!", It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result.Success<UserAccount, Error>(user));
        var tokenProvider = new Mock<ITokenProvider>();
        tokenProvider.Setup(item => item.GenerateAccessTokenAsync(user, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new JwtTokenResult("access", Guid.NewGuid()));
        tokenProvider.Setup(item => item.GenerateRefreshTokenAsync(user, It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result.Success<string, Error>("refresh"));
        var handler = new LoginHandler(new LoginCommandValidator(), accounts.Object, tokenProvider.Object, NullLogger<LoginHandler>.Instance);

        var result = await handler.Handle(new LoginCommand("user@example.com", "Password1!"));

        Assert.True(result.IsSuccess);
        Assert.Equal(new AuthTokens("access", "refresh"), result.Value);
    }

    [Fact]
    public async Task Handle_WhenCredentialsAreInvalid_ReturnsCredentialsInvalid()
    {
        var accounts = new Mock<IUserAccountService>();
        accounts.Setup(item => item.AuthenticateAsync("user@example.com", "Incorrect1!", It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result.Failure<UserAccount, Error>(AuthErrors.CredentialsInvalid()));
        var result = await new LoginHandler(new LoginCommandValidator(), accounts.Object, Mock.Of<ITokenProvider>(), NullLogger<LoginHandler>.Instance)
            .Handle(new LoginCommand("user@example.com", "Incorrect1!"));
        Assert.True(result.IsFailure);
        Assert.Equal(AuthErrorCodes.CredentialsInvalid, result.Error[0].Code);
    }

    [Fact]
    public async Task Handle_WhenEmailIsNotConfirmed_ReturnsEmailNotConfirmedWithoutGeneratingTokens()
    {
        var user = new UserAccount(Guid.NewGuid(), "user@example.com", false);
        var accounts = new Mock<IUserAccountService>();
        accounts.Setup(item => item.AuthenticateAsync("user@example.com", "Password1!", It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result.Success<UserAccount, Error>(user));
        var tokens = new Mock<ITokenProvider>();
        var handler = new LoginHandler(new LoginCommandValidator(), accounts.Object, tokens.Object, NullLogger<LoginHandler>.Instance);

        var result = await handler.Handle(new LoginCommand("user@example.com", "Password1!"));

        Assert.True(result.IsFailure);
        Assert.Equal(UsersApplicationErrorCodes.EmailNotConfirmed, result.Error[0].Code);
        Assert.Equal(ErrorType.Unauthorized, result.Error[0].Type);
        Assert.Equal("email", result.Error[0].Target);
        tokens.Verify(item => item.GenerateAccessTokenAsync(It.IsAny<UserAccount>(), It.IsAny<CancellationToken>()), Times.Never);
        tokens.Verify(item => item.GenerateRefreshTokenAsync(It.IsAny<UserAccount>(), It.IsAny<Guid>(), It.IsAny<CancellationToken>()), Times.Never);
    }
}
