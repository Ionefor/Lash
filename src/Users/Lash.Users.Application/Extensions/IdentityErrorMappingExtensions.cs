using ErrorsFlow.Errors;
using ErrorsFlow.Models;
using Lash.Users.Application.Features.Commands.RegisterClient;
using Microsoft.AspNetCore.Identity;

namespace Lash.Users.Application.Extensions;

internal static class IdentityErrorMappingExtensions
{
    public static ErrorList ToErrorList(this IdentityResult identityResult)
    {
        ArgumentNullException.ThrowIfNull(identityResult);

        return new ErrorList(identityResult.Errors.Select(Map));
    }

    private static Error Map(IdentityError error) => error.Code switch
    {
        nameof(IdentityErrorDescriber.DuplicateUserName) or
        nameof(IdentityErrorDescriber.DuplicateEmail) =>
            GeneralErrors.ValueAlreadyExists(nameof(RegisterClientCommand.Email)),

        nameof(IdentityErrorDescriber.InvalidEmail) or
        nameof(IdentityErrorDescriber.InvalidUserName) =>
            GeneralErrors.ValueIsInvalid(nameof(RegisterClientCommand.Email)),

        nameof(IdentityErrorDescriber.PasswordTooShort) or
        nameof(IdentityErrorDescriber.PasswordRequiresNonAlphanumeric) or
        nameof(IdentityErrorDescriber.PasswordRequiresDigit) or
        nameof(IdentityErrorDescriber.PasswordRequiresLower) or
        nameof(IdentityErrorDescriber.PasswordRequiresUpper) or
        nameof(IdentityErrorDescriber.PasswordRequiresUniqueChars) =>
            GeneralErrors.ValueIsInvalid(nameof(RegisterClientCommand.Password)),

        _ => GeneralErrors.Failed("Unable to register user.")
    };
}
