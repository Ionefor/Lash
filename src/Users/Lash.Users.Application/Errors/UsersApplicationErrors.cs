using ErrorsFlow;
using ErrorsFlow.Models;

namespace Lash.Users.Application.Errors;

public static class UsersApplicationErrorCodes
{
    public const string RequiredRoleNotConfigured = "users.required_role.not_configured";
    public const string EmailConfirmationCodeInvalid = "users.email_confirmation.code_invalid";
    public const string EmailNotConfirmed = "users.email.not_confirmed";
    public const string PasswordResetCodeInvalid = "users.password_reset.code_invalid";
}

public static class UsersApplicationErrors
{
    public static Error RequiredRoleNotConfigured() =>
        ErrorFactory.Create(
            UsersApplicationErrorCodes.RequiredRoleNotConfigured,
            "Registration is temporarily unavailable.",
            ErrorType.Failure,
            "role");

    public static Error EmailConfirmationCodeInvalid() => ErrorFactory.Create(
        UsersApplicationErrorCodes.EmailConfirmationCodeInvalid,
        "The confirmation code is invalid or expired.",
        ErrorType.Validation,
        "code");

    public static Error EmailNotConfirmed() => ErrorFactory.Create(
        UsersApplicationErrorCodes.EmailNotConfirmed,
        "Email confirmation is required.",
        ErrorType.Unauthorized,
        "email");

    public static Error PasswordResetCodeInvalid() => ErrorFactory.Create(
        UsersApplicationErrorCodes.PasswordResetCodeInvalid,
        "The password reset code is invalid or expired.",
        ErrorType.Validation,
        "code");
}
