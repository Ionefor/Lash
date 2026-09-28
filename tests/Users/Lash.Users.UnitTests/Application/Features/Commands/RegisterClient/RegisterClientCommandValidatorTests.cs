using Lash.Users.Application.Features.Commands.RegisterClient;

namespace Lash.Users.UnitTests.Application.Features.Commands.RegisterClient;

public sealed class RegisterClientCommandValidatorTests
{
    private readonly RegisterClientCommandValidator _validator = new();

    [Fact]
    public void Validate_WhenCommandIsValid_ReturnsSuccess()
    {
        var command = new RegisterClientCommand(
            "client@example.com",
            "Password1!",
            "Password1!");

        var result = _validator.Validate(command);

        Assert.True(result.IsValid);
    }

    [Theory]
    [InlineData("password1!", "users.password.uppercase_required")]
    [InlineData("PASSWORD1!", "users.password.lowercase_required")]
    [InlineData("Password!", "users.password.digit_required")]
    [InlineData("Password1", "users.password.special_character_required")]
    [InlineData("Pass1!", "users.password.min_length")]
    [InlineData("Пароль1!", "users.password.ascii_only")]
    public void Validate_WhenPasswordViolatesRule_ReturnsExpectedErrorCode(
        string password,
        string expectedErrorCode)
    {
        var command = new RegisterClientCommand(
            "client@example.com",
            password,
            password);

        var result = _validator.Validate(command);

        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, error => error.ErrorCode == expectedErrorCode);
    }

    [Fact]
    public void Validate_WhenPasswordConfirmationDoesNotMatch_ReturnsMismatchError()
    {
        var command = new RegisterClientCommand(
            "client@example.com",
            "Password1!",
            "Password2!");

        var result = _validator.Validate(command);

        Assert.Contains(result.Errors, error =>
            error.ErrorCode == "users.password.confirmation_mismatch");
    }

    [Fact]
    public void Validate_WhenPasswordIsNull_ReturnsRequiredError()
    {
        var command = new RegisterClientCommand(
            "client@example.com",
            null!,
            "Password1!");

        var result = _validator.Validate(command);

        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, error => error.ErrorCode == "users.password.required");
    }

    [Fact]
    public void Validate_WhenPasswordIsEmpty_ReturnsOnlyRequiredError()
    {
        var command = new RegisterClientCommand(
            "client@example.com",
            string.Empty,
            "Password1!");

        var result = _validator.Validate(command);

        var passwordErrors = result.Errors.Where(error => error.PropertyName == "Password").ToArray();
        var error = Assert.Single(passwordErrors);
        Assert.Equal("users.password.required", error.ErrorCode);
    }

    [Fact]
    public void Validate_WhenPasswordConfirmationIsEmpty_ReturnsOnlyRequiredError()
    {
        var command = new RegisterClientCommand(
            "client@example.com",
            "Password1!",
            string.Empty);

        var result = _validator.Validate(command);

        var confirmationErrors = result.Errors.Where(error => error.PropertyName == "ConfirmPassword").ToArray();
        var error = Assert.Single(confirmationErrors);
        Assert.Equal("users.password.confirmation_required", error.ErrorCode);
    }

    [Fact]
    public void Validate_WhenEmailIsEmpty_ReturnsOnlyRequiredError()
    {
        var command = new RegisterClientCommand(
            string.Empty,
            "Password1!",
            "Password1!");

        var result = _validator.Validate(command);

        var emailErrors = result.Errors.Where(error => error.PropertyName == "Email").ToArray();
        var error = Assert.Single(emailErrors);
        Assert.Equal("users.email.required", error.ErrorCode);
    }
}
