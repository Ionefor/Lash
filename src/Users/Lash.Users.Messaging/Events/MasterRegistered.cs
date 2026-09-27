namespace Lash.Users.Messaging.Events;

public sealed record MasterRegistered(
    Guid EventId,
    Guid UserId,
    DateTimeOffset OccurredAt) : IUsersEvent;
