using WebFlow.Abstractions.Interfaces;

namespace Lash.Users.Application.Features.Queries.GetCurrentUser;

public sealed record GetCurrentUserQuery(Guid UserId) : IQuery;
