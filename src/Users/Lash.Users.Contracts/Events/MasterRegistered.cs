namespace Lash.Users.Contracts.Events;

public sealed record MasterRegistered(
    Guid EventId,
    Guid UserId,
    DateTimeOffset OccurredAt) : IUsersEvent;
