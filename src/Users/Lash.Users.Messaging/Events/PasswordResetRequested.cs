namespace Lash.Users.Messaging.Events;

public sealed record PasswordResetRequested(
    Guid EventId,
    Guid UserId,
    DateTimeOffset OccurredAt) : IUsersEvent;
