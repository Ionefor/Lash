using CSharpFunctionalExtensions;
using ErrorsFlow.Errors;
using ErrorsFlow.Models;
using FluentValidation;
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

    public RegisterClientHandler(
        IValidator<RegisterClientCommand> validator,
        UserManager<User> userManager,
        RoleManager<Role> roleManager)
    {
        _validator = validator;
        _userManager = userManager;
        _roleManager = roleManager;
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
            return GeneralErrors.NotFound(nameof(Role), RoleNames.Client).ToErrorList();
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

        return userResult.Value.Id;
    }
}
