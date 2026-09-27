using FluentValidation;
using Lash.Users.Application.Errors;

namespace Lash.Users.Application.Extensions;

internal static class EmailValidationExtensions
{
    public static IRuleBuilderOptions<T, string> MustBeValidEmail<T>(
        this IRuleBuilder<T, string> ruleBuilder)
    {
        return ruleBuilder
            .NotEmpty()
            .WithErrorCode(UsersValidationErrorCodes.EmailRequired)
            .WithMessage("Email must be provided.")
            .EmailAddress()
            .WithErrorCode(UsersValidationErrorCodes.EmailInvalid)
            .WithMessage("Email must be valid.")
            .MaximumLength(256)
            .WithErrorCode(UsersValidationErrorCodes.EmailMaxLength)
            .WithMessage("Email must contain at most 256 characters.");
    }
}
