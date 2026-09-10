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
    [InlineData("password1!", "password.uppercase_required")]
    [InlineData("PASSWORD1!", "password.lowercase_required")]
    [InlineData("Password!", "password.digit_required")]
    [InlineData("Password1", "password.special_character_required")]
    [InlineData("Pass1!", "password.min_length")]
    [InlineData("Пароль1!", "password.ascii_only")]
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
            error.ErrorCode == "password.confirmation_mismatch");
    }
}
