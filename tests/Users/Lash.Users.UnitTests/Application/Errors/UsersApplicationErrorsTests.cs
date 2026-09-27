using ErrorsFlow;
using ErrorsFlow.Models;
using Lash.Users.Application.Errors;

namespace Lash.Users.UnitTests.Application.Errors;

public sealed class UsersApplicationErrorsTests
{
    [Theory]
    [InlineData(UsersApplicationErrorCodes.RequiredRoleNotConfigured, ErrorType.Failure, "role")]
    [InlineData(UsersApplicationErrorCodes.EmailConfirmationCodeInvalid, ErrorType.Validation, "code")]
    [InlineData(UsersApplicationErrorCodes.EmailNotConfirmed, ErrorType.Unauthorized, "email")]
    [InlineData(UsersApplicationErrorCodes.PasswordResetCodeInvalid, ErrorType.Validation, "code")]
    public void Error_WithTarget_HasStableCodeTypeAndTarget(string code, ErrorType type, string target)
    {
        var error = code switch
        {
            UsersApplicationErrorCodes.RequiredRoleNotConfigured => UsersApplicationErrors.RequiredRoleNotConfigured(),
            UsersApplicationErrorCodes.EmailConfirmationCodeInvalid => UsersApplicationErrors.EmailConfirmationCodeInvalid(),
            UsersApplicationErrorCodes.EmailNotConfirmed => UsersApplicationErrors.EmailNotConfirmed(),
            UsersApplicationErrorCodes.PasswordResetCodeInvalid => UsersApplicationErrors.PasswordResetCodeInvalid(),
            _ => throw new ArgumentOutOfRangeException(nameof(code), code, null)
        };

        Assert.Equal(code, error.Code);
        Assert.Equal(type, error.Type);
        Assert.Equal(target, error.Target);
    }

    [Fact]
    public void EmailDeliveryFailed_HasStableCodeAndType()
    {
        var error = UsersApplicationErrors.EmailDeliveryFailed();

        Assert.Equal(UsersApplicationErrorCodes.EmailDeliveryFailed, error.Code);
        Assert.Equal(ErrorType.Failure, error.Type);
        Assert.Null(error.Target);
    }
}
