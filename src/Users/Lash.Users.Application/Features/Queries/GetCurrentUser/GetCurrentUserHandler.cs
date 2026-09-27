using CSharpFunctionalExtensions;
using ErrorsFlow.Errors;
using ErrorsFlow.Models;
using Lash.Users.Application.Abstractions;
using Lash.Users.Application.Models;
using FluentValidation;
using Microsoft.Extensions.Logging;
using WebFlow.Abstractions.Interfaces;
using WebFlow.FluentValidation.Extensions;

namespace Lash.Users.Application.Features.Queries.GetCurrentUser;

public sealed class GetCurrentUserHandler(
    IValidator<GetCurrentUserQuery> validator,
    IUserAccountService accounts,
    ILogger<GetCurrentUserHandler> logger)
    : IQueryHandler<GetCurrentUserQuery, UserProfile>
{
    public async Task<Result<UserProfile, ErrorList>> Handle(
        GetCurrentUserQuery query,
        CancellationToken cancellationToken = default)
    {
        var validation = await validator.ValidateAsync(query, cancellationToken);
        if (!validation.IsValid)
        {
            logger.LogWarning("Current user query validation failed.");
            return validation.ToErrorList();
        }

        var user = await accounts.FindByIdAsync(query.UserId, cancellationToken);
        if (user is null || string.IsNullOrWhiteSpace(user.Email))
        {
            logger.LogWarning("Current user query could not find user {UserId}.", query.UserId);
            return GeneralErrors.NotFound("UserAccount", nameof(query.UserId)).ToErrorList();
        }

        var roles = await accounts.GetRolesAsync(user.Id, cancellationToken);
        return new UserProfile(user.Id, user.Email, roles);
    }
}
