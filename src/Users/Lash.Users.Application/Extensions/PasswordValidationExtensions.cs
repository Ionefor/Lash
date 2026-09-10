using FluentValidation;

namespace Lash.Users.Application.Extensions;

internal static class PasswordValidationExtensions
{
    public static IRuleBuilderOptions<T, string> MustBeStrongPassword<T>(
        this IRuleBuilder<T, string> ruleBuilder)
    {
        return ruleBuilder
            .NotEmpty()
            .WithErrorCode(PasswordErrorCodes.Required)
            .WithMessage("Password must be provided.")
            .MinimumLength(8)
            .WithErrorCode(PasswordErrorCodes.MinLength)
            .WithMessage("Password must contain at least 8 characters.")
            .MaximumLength(32)
            .WithErrorCode(PasswordErrorCodes.MaxLength)
            .WithMessage("Password must contain at most 32 characters.")
            .Must(password => password.All(character => character is >= '!' and <= '~'))
            .WithErrorCode(PasswordErrorCodes.AsciiOnly)
            .WithMessage("Password must contain only English letters, digits, and special characters.")
            .Must(password => password.Any(char.IsUpper))
            .WithErrorCode(PasswordErrorCodes.UppercaseRequired)
            .WithMessage("Password must contain an uppercase letter.")
            .Must(password => password.Any(char.IsLower))
            .WithErrorCode(PasswordErrorCodes.LowercaseRequired)
            .WithMessage("Password must contain a lowercase letter.")
            .Must(password => password.Any(char.IsDigit))
            .WithErrorCode(PasswordErrorCodes.DigitRequired)
            .WithMessage("Password must contain a digit.")
            .Must(password => password.Any(character =>
                !char.IsLetterOrDigit(character) && !char.IsWhiteSpace(character)))
            .WithErrorCode(PasswordErrorCodes.SpecialCharacterRequired)
            .WithMessage("Password must contain a special character.");
    }

    public static IRuleBuilderOptions<T, string> MustMatchPassword<T>(
        this IRuleBuilder<T, string> ruleBuilder,
        Func<T, string> passwordSelector)
    {
        return ruleBuilder
            .NotEmpty()
            .WithErrorCode(PasswordErrorCodes.ConfirmationRequired)
            .WithMessage("Password confirmation must be provided.")
            .Must((command, confirmation) => string.Equals(
                confirmation,
                passwordSelector(command),
                StringComparison.Ordinal))
            .WithErrorCode(PasswordErrorCodes.ConfirmationMismatch)
            .WithMessage("Passwords must match.");
    }
}

internal static class PasswordErrorCodes
{
    public const string Required = "password.required";
    public const string MinLength = "password.min_length";
    public const string MaxLength = "password.max_length";
    public const string AsciiOnly = "password.ascii_only";
    public const string UppercaseRequired = "password.uppercase_required";
    public const string LowercaseRequired = "password.lowercase_required";
    public const string DigitRequired = "password.digit_required";
    public const string SpecialCharacterRequired = "password.special_character_required";
    public const string ConfirmationRequired = "password.confirmation_required";
    public const string ConfirmationMismatch = "password.confirmation_mismatch";
}
