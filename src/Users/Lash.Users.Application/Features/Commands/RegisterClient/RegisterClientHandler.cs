using CSharpFunctionalExtensions;
using ErrorsFlow.Models;
using FluentValidation;
using Lash.Users.Application.Errors;
using Lash.Users.Application.Abstractions;
using Lash.Users.Application.Extensions;
using Lash.Users.Domain;
using Microsoft.AspNetCore.Identity;
using WebFlow.Abstractions.Interfaces;
using WebFlow.FluentValidation.Extensions;

namespace Lash.Users.Application.Features.Commands.RegisterClient;

public sealed class RegisterClientHandler : ICommandHandler<RegisterClientCommand, Guid>
{
    private readonly IValidator<RegisterClientCommand> _validator;
    private readonly UserManager<User> _userManager;
    private readonly RoleManager<Role> _roleManager;
    private readonly IEmailConfirmationSender _emailConfirmationSender;

    public RegisterClientHandler(
        IValidator<RegisterClientCommand> validator,
        UserManager<User> userManager,
        RoleManager<Role> roleManager,
        IEmailConfirmationSender emailConfirmationSender)
    {
        _validator = validator;
        _userManager = userManager;
        _roleManager = roleManager;
        _emailConfirmationSender = emailConfirmationSender;
    }

    public async Task<Result<Guid, ErrorList>> Handle(
        RegisterClientCommand command,
        CancellationToken cancellationToken = default)
    {
        var validationResult = await _validator.ValidateAsync(command, cancellationToken);

        if (!validationResult.IsValid)
        {
            return validationResult.ToErrorList();
        }

        var clientRole = await _roleManager.FindByNameAsync(RoleNames.Client);

        if (clientRole is null)
        {
            return UsersApplicationErrors.RequiredRoleNotConfigured().ToErrorList();
        }

        var userResult = User.RegisterClient(command.Email, clientRole);

        if (userResult.IsFailure)
        {
            return userResult.Error.ToErrorList();
        }

        var identityResult = await _userManager.CreateAsync(userResult.Value, command.Password);

        if (!identityResult.Succeeded)
        {
            return identityResult.ToErrorList();
        }

        var emailResult = await _emailConfirmationSender.SendAsync(userResult.Value, cancellationToken);
        return emailResult.IsSuccess ? userResult.Value.Id : emailResult.Error.ToErrorList();
    }
}
