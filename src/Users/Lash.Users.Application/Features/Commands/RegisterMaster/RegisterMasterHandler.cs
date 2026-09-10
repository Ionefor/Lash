using CSharpFunctionalExtensions;
using ErrorsFlow.Errors;
using ErrorsFlow.Models;
using FluentValidation;
using Lash.Users.Application.Extensions;
using Lash.Users.Domain;
using Microsoft.AspNetCore.Identity;
using WebFlow.Abstractions.Interfaces;
using WebFlow.FluentValidation.Extensions;

namespace Lash.Users.Application.Features.Commands.RegisterMaster;

public sealed class RegisterMasterHandler : ICommandHandler<RegisterMasterCommand, Guid>
{
    private readonly IValidator<RegisterMasterCommand> _validator;
    private readonly UserManager<User> _userManager;
    private readonly RoleManager<Role> _roleManager;

    public RegisterMasterHandler(
        IValidator<RegisterMasterCommand> validator,
        UserManager<User> userManager,
        RoleManager<Role> roleManager)
    {
        _validator = validator;
        _userManager = userManager;
        _roleManager = roleManager;
    }

    public async Task<Result<Guid, ErrorList>> Handle(
        RegisterMasterCommand command,
        CancellationToken cancellationToken = default)
    {
        var validationResult = await _validator.ValidateAsync(command, cancellationToken);

        if (!validationResult.IsValid)
        {
            return validationResult.ToErrorList();
        }

        var masterRole = await _roleManager.FindByNameAsync(RoleNames.Master);

        if (masterRole is null)
        {
            return GeneralErrors.NotFound(nameof(Role), RoleNames.Master).ToErrorList();
        }

        var userResult = User.RegisterMaster(command.Email, masterRole);

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
