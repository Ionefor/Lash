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

public static class UsersValidationErrorCodes
{
    public const string EmailRequired = "users.email.required";
    public const string EmailInvalid = "users.email.invalid";
    public const string EmailMaxLength = "users.email.max_length";
    public const string PasswordRequired = "users.password.required";
    public const string PasswordMinLength = "users.password.min_length";
    public const string PasswordMaxLength = "users.password.max_length";
    public const string PasswordAsciiOnly = "users.password.ascii_only";
    public const string PasswordUppercaseRequired = "users.password.uppercase_required";
    public const string PasswordLowercaseRequired = "users.password.lowercase_required";
    public const string PasswordDigitRequired = "users.password.digit_required";
    public const string PasswordSpecialCharacterRequired = "users.password.special_character_required";
    public const string PasswordConfirmationRequired = "users.password.confirmation_required";
    public const string PasswordConfirmationMismatch = "users.password.confirmation_mismatch";
    public const string CurrentPasswordRequired = "users.current_password.required";
    public const string UserIdRequired = "users.user_id.required";
    public const string EmailConfirmationCodeRequired = "users.email_confirmation.code.required";
    public const string PasswordResetCodeRequired = "users.password_reset.code.required";
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
