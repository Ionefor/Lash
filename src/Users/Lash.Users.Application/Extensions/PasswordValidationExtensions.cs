using FluentValidation;
using Lash.Users.Application.Errors;

namespace Lash.Users.Application.Extensions;

internal static class PasswordValidationExtensions
{
    public static IRuleBuilderOptions<T, string> MustBeStrongPassword<T>(
        this IRuleBuilderInitial<T, string> ruleBuilder)
    {
        return ruleBuilder
            .Cascade(CascadeMode.Stop)
            .NotEmpty()
            .WithErrorCode(UsersValidationErrorCodes.PasswordRequired)
            .WithMessage("Password must be provided.")
            .MinimumLength(8)
            .WithErrorCode(UsersValidationErrorCodes.PasswordMinLength)
            .WithMessage("Password must contain at least 8 characters.")
            .MaximumLength(32)
            .WithErrorCode(UsersValidationErrorCodes.PasswordMaxLength)
            .WithMessage("Password must contain at most 32 characters.")
            .Must(password => password is not null && password.All(character => character is >= '!' and <= '~'))
            .WithErrorCode(UsersValidationErrorCodes.PasswordAsciiOnly)
            .WithMessage("Password must contain only English letters, digits, and special characters.")
            .Must(password => password is not null && password.Any(char.IsUpper))
            .WithErrorCode(UsersValidationErrorCodes.PasswordUppercaseRequired)
            .WithMessage("Password must contain an uppercase letter.")
            .Must(password => password is not null && password.Any(char.IsLower))
            .WithErrorCode(UsersValidationErrorCodes.PasswordLowercaseRequired)
            .WithMessage("Password must contain a lowercase letter.")
            .Must(password => password is not null && password.Any(char.IsDigit))
            .WithErrorCode(UsersValidationErrorCodes.PasswordDigitRequired)
            .WithMessage("Password must contain a digit.")
            .Must(password => password is not null && password.Any(character =>
                !char.IsLetterOrDigit(character) && !char.IsWhiteSpace(character)))
            .WithErrorCode(UsersValidationErrorCodes.PasswordSpecialCharacterRequired)
            .WithMessage("Password must contain a special character.");
    }

    public static IRuleBuilderOptions<T, string> MustMatchPassword<T>(
        this IRuleBuilderInitial<T, string> ruleBuilder,
        Func<T, string> passwordSelector)
    {
        return ruleBuilder
            .Cascade(CascadeMode.Stop)
            .NotEmpty()
            .WithErrorCode(UsersValidationErrorCodes.PasswordConfirmationRequired)
            .WithMessage("Password confirmation must be provided.")
            .Must((command, confirmation) => string.Equals(
                confirmation,
                passwordSelector(command),
                StringComparison.Ordinal))
            .WithErrorCode(UsersValidationErrorCodes.PasswordConfirmationMismatch)
            .WithMessage("Passwords must match.");
    }
}
