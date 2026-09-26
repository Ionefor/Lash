using CSharpFunctionalExtensions;
using ErrorsFlow.Errors;
using ErrorsFlow.Models;
using FluentValidation;
using Lash.Users.Application.Abstractions;
using Lash.Users.Application.Models;
using Lash.Users.Application.Errors;
using Lash.Users.Domain;
using Microsoft.AspNetCore.Identity;
using WebFlow.Abstractions.Interfaces;
using WebFlow.FluentValidation.Extensions;

namespace Lash.Users.Application.Features.Commands.Login;

public sealed class LoginHandler(
    IValidator<LoginCommand> validator,
    UserManager<User> userManager,
    ITokenProvider tokenProvider) : ICommandHandler<LoginCommand, AuthTokens>
{
    public async Task<Result<AuthTokens, ErrorList>> Handle(LoginCommand command, CancellationToken cancellationToken = default)
    {
        var validation = await validator.ValidateAsync(command, cancellationToken);
        if (!validation.IsValid)
            return validation.ToErrorList();

        var user = await userManager.FindByEmailAsync(command.Email);
        if (user is null || await userManager.IsLockedOutAsync(user))
            return AuthErrors.CredentialsInvalid().ToErrorList();

        if (!await userManager.CheckPasswordAsync(user, command.Password))
        {
            var accessFailed = await userManager.AccessFailedAsync(user);
            return accessFailed.Succeeded
                ? AuthErrors.CredentialsInvalid().ToErrorList()
                : GeneralErrors.Failed("Unable to process login.").ToErrorList();
        }

        var resetAccessFailedCount = await userManager.ResetAccessFailedCountAsync(user);
        if (!resetAccessFailedCount.Succeeded)
            return GeneralErrors.Failed("Unable to process login.").ToErrorList();

        if (!user.EmailConfirmed)
            return UsersApplicationErrors.EmailNotConfirmed().ToErrorList();

        var accessToken = await tokenProvider.GenerateAccessTokenAsync(user, cancellationToken);
        var refreshToken = await tokenProvider.GenerateRefreshTokenAsync(user, accessToken.Jti, cancellationToken);
        return refreshToken.IsFailure
            ? refreshToken.Error.ToErrorList()
            : new AuthTokens(accessToken.AccessToken, refreshToken.Value);
    }
}
