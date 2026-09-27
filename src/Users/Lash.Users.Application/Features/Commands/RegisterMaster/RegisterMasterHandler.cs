using CSharpFunctionalExtensions;
using ErrorsFlow.Models;
using FluentValidation;
using Lash.Users.Application.Errors;
using Lash.Users.Application.Abstractions;
using Lash.Users.Application.Constants;
using Lash.Users.Application.Extensions;
using Lash.Users.Application.Models;
using Lash.Users.Messaging.Events;
using Microsoft.Extensions.Logging;
using WebFlow.Abstractions.Interfaces;
using WebFlow.FluentValidation.Extensions;

namespace Lash.Users.Application.Features.Commands.RegisterMaster;

public sealed class RegisterMasterHandler : ICommandHandler<RegisterMasterCommand, Guid>
{
    private readonly IValidator<RegisterMasterCommand> _validator;
    private readonly IUserAccountService _accounts;
    private readonly IUsersEventPublisher _eventPublisher;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<RegisterMasterHandler> _logger;

    public RegisterMasterHandler(
        IValidator<RegisterMasterCommand> validator,
        IUserAccountService accounts,
        IUsersEventPublisher eventPublisher,
        IUnitOfWork unitOfWork,
        ILogger<RegisterMasterHandler> logger)
    {
        _validator = validator;
        _accounts = accounts;
        _eventPublisher = eventPublisher;
        _unitOfWork = unitOfWork;
        _logger = logger;
    }

    public async Task<Result<Guid, ErrorList>> Handle(
        RegisterMasterCommand command,
        CancellationToken cancellationToken = default)
    {
        var validationResult = await _validator.ValidateAsync(command, cancellationToken);

        if (!validationResult.IsValid)
        {
            _logger.LogWarning("Master registration validation failed.");
            return validationResult.ToErrorList();
        }

        if (!await _accounts.RoleExistsAsync(AccountRoleNames.Master, cancellationToken))
        {
            _logger.LogError("Master registration cannot proceed because the required role is not configured.");
            return UsersApplicationErrors.RequiredRoleNotConfigured().ToErrorList();
        }

        await using var transaction = await _unitOfWork.BeginTransactionAsync(cancellationToken);
        var creation = await _accounts.CreateAsync(command.Email, command.Password, AccountRoleNames.Master, cancellationToken);
        if (creation.IsFailure)
        {
            _logger.LogWarning("Master registration could not create an account.");
            return creation.Error.ToErrorList();
        }

        await _eventPublisher.PublishAsync(new MasterRegistered(
            Guid.NewGuid(),
            creation.Value.Id,
            DateTimeOffset.UtcNow), cancellationToken);
        await _eventPublisher.PublishAsync(new EmailConfirmationRequested(
            Guid.NewGuid(),
            creation.Value.Id,
            DateTimeOffset.UtcNow), cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
        await transaction.CommitAsync(cancellationToken);

        _logger.LogInformation("Master registration completed for user {UserId}.", creation.Value.Id);
        return creation.Value.Id;
    }
}
