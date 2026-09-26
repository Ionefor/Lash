using CSharpFunctionalExtensions;
using ErrorsFlow.Errors;
using ErrorsFlow.Models;
using Lash.Users.Application.Models;
using Lash.Users.Domain;
using Microsoft.AspNetCore.Identity;
using WebFlow.Abstractions.Interfaces;

namespace Lash.Users.Application.Features.Queries.GetCurrentUser;

public sealed class GetCurrentUserHandler(UserManager<User> userManager)
    : IQueryHandler<GetCurrentUserQuery, UserProfile>
{
    public async Task<Result<UserProfile, ErrorList>> Handle(
        GetCurrentUserQuery query,
        CancellationToken cancellationToken = default)
    {
        var user = await userManager.FindByIdAsync(query.UserId.ToString());
        if (user is null || string.IsNullOrWhiteSpace(user.Email))
            return GeneralErrors.NotFound(nameof(User), nameof(query.UserId)).ToErrorList();

        var roles = await userManager.GetRolesAsync(user);
        return new UserProfile(user.Id, user.Email, roles.ToArray());
    }
}
