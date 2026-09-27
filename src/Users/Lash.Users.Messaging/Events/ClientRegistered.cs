namespace Lash.Users.Messaging.Events;

public sealed record ClientRegistered(
    Guid EventId,
    Guid UserId,
    DateTimeOffset OccurredAt);
