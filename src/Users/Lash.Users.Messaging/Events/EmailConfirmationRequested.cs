namespace Lash.Users.Messaging.Events;

public sealed record EmailConfirmationRequested(
    Guid EventId,
    Guid UserId,
    DateTimeOffset OccurredAt);
