using CSharpFunctionalExtensions;
using ErrorsFlow.Models;
using FluentValidation;
using Lash.Users.Application.Errors;
using Lash.Users.Application.Abstractions;
using Lash.Users.Application.Constants;
using Lash.Users.Application.Extensions;
using Lash.Users.Application.Models;
using Lash.Users.Contracts.Events;
using Microsoft.Extensions.Logging;
using WebFlow.Abstractions.Interfaces;
using WebFlow.FluentValidation.Extensions;

namespace Lash.Users.Application.Features.Commands.RegisterClient;

public sealed class RegisterClientHandler : ICommandHandler<RegisterClientCommand, Guid>
{
    private readonly IValidator<RegisterClientCommand> _validator;
    private readonly IUserAccountService _accounts;
    private readonly IUsersEventPublisher _eventPublisher;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<RegisterClientHandler> _logger;
    private readonly TimeProvider _timeProvider;

    public RegisterClientHandler(IValidator<RegisterClientCommand> validator,
        IUserAccountService accounts,
        IUsersEventPublisher eventPublisher,
        IUnitOfWork unitOfWork,
        ILogger<RegisterClientHandler> logger, TimeProvider timeProvider)
    {
        _validator = validator;
        _accounts = accounts;
        _eventPublisher = eventPublisher;
        _unitOfWork = unitOfWork;
        _logger = logger;
        _timeProvider = timeProvider;
    }

    public async Task<Result<Guid, ErrorList>> Handle(
        RegisterClientCommand command,
        CancellationToken cancellationToken = default)
    {
        var validationResult = await _validator.ValidateAsync(command, cancellationToken);

        if (!validationResult.IsValid)
        {
            _logger.LogWarning("Client registration validation failed.");
            return validationResult.ToErrorList();
        }

        if (!await _accounts.RoleExistsAsync(AccountRoleNames.Client, cancellationToken))
        {
            _logger.LogError("Client registration cannot proceed because the required role is not configured.");
            return UsersApplicationErrors.RequiredRoleNotConfigured().ToErrorList();
        }

        await using var transaction = await _unitOfWork.BeginTransactionAsync(cancellationToken);
        var creation = await _accounts.CreateAsync(command.Email, command.Password, AccountRoleNames.Client, cancellationToken);
        if (creation.IsFailure)
        {
            _logger.LogWarning("Client registration could not create an account.");
            return creation.Error.ToErrorList();
        }

        await _eventPublisher.PublishAsync(new ClientRegistered(
            Guid.NewGuid(),
            creation.Value.Id,
            _timeProvider.GetUtcNow()), cancellationToken);
        await _eventPublisher.PublishAsync(new EmailConfirmationRequested(
            Guid.NewGuid(),
            creation.Value.Id,
            _timeProvider.GetUtcNow()), cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
        await transaction.CommitAsync(cancellationToken);

        _logger.LogInformation("Client registration completed for user {UserId}.", creation.Value.Id);
        return creation.Value.Id;
    }
}
