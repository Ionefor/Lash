namespace Lash.Users.Contracts.Events;

public sealed record ClientRegistered(
    Guid EventId,
    Guid UserId,
    DateTimeOffset OccurredAt) : IUsersEvent;
