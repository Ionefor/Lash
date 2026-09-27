using Lash.Users.Application.Features.Commands.ConfirmEmail;
using Lash.Users.Application.Features.Commands.Login;
using Lash.Users.Application.Features.Commands.RequestPasswordReset;
using Lash.Users.Application.Features.Commands.ResendEmailConfirmation;
using Lash.Users.Application.Features.Queries.GetCurrentUser;

namespace Lash.Users.UnitTests.Application.Features.Validation;

public sealed class IdentityCommandValidatorsTests
{
    [Theory]
    [InlineData("", "users.email.required")]
    [InlineData("invalid", "users.email.invalid")]
    [InlineData("aaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaa@example.com", "users.email.max_length")]
    public void Validate_WhenEmailIsInvalid_ReturnsExpectedErrorCode(string email, string expectedErrorCode)
    {
        var validators = new IValidator[]
        {
            new RegisterClientEmailValidator(),
            new LoginEmailValidator(),
            new ConfirmEmailEmailValidator(),
            new ResendEmailEmailValidator(),
            new PasswordResetRequestEmailValidator()
        };

        foreach (var validator in validators)
        {
            var result = validator.Validate(email);

            Assert.Contains(result.Errors, error => error.ErrorCode == expectedErrorCode);
        }
    }

    [Fact]
    public void Validate_WhenConfirmationCodeIsEmpty_ReturnsExpectedErrorCode()
    {
        var result = new ConfirmEmailCommandValidator().Validate(new ConfirmEmailCommand("user@example.com", ""));

        Assert.Contains(result.Errors, error => error.ErrorCode == "users.email_confirmation.code.required");
    }

    [Fact]
    public void Validate_WhenUserIdIsEmpty_ReturnsExpectedErrorCode()
    {
        var result = new GetCurrentUserQueryValidator().Validate(new GetCurrentUserQuery(Guid.Empty));

        Assert.Contains(result.Errors, error => error.ErrorCode == "users.user_id.required");
    }

    private interface IValidator
    {
        FluentValidation.Results.ValidationResult Validate(string email);
    }

    private sealed class RegisterClientEmailValidator : IValidator
    {
        public FluentValidation.Results.ValidationResult Validate(string email) =>
            new Lash.Users.Application.Features.Commands.RegisterClient.RegisterClientCommandValidator()
                .Validate(new Lash.Users.Application.Features.Commands.RegisterClient.RegisterClientCommand(email, "Password1!", "Password1!"));
    }

    private sealed class LoginEmailValidator : IValidator
    {
        public FluentValidation.Results.ValidationResult Validate(string email) =>
            new LoginCommandValidator().Validate(new LoginCommand(email, "Password1!"));
    }

    private sealed class ConfirmEmailEmailValidator : IValidator
    {
        public FluentValidation.Results.ValidationResult Validate(string email) =>
            new ConfirmEmailCommandValidator().Validate(new ConfirmEmailCommand(email, "code"));
    }

    private sealed class ResendEmailEmailValidator : IValidator
    {
        public FluentValidation.Results.ValidationResult Validate(string email) =>
            new ResendEmailConfirmationCommandValidator().Validate(new ResendEmailConfirmationCommand(email));
    }

    private sealed class PasswordResetRequestEmailValidator : IValidator
    {
        public FluentValidation.Results.ValidationResult Validate(string email) =>
            new RequestPasswordResetCommandValidator().Validate(new RequestPasswordResetCommand(email));
    }
}
