namespace Lash.Users.Contracts.Events;

public sealed record PasswordResetRequested(
    Guid EventId,
    Guid UserId,
    DateTimeOffset OccurredAt) : IUsersEvent;
