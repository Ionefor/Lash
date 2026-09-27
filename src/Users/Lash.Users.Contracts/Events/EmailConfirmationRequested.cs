namespace Lash.Users.Contracts.Events;

public sealed record EmailConfirmationRequested(
    Guid EventId,
    Guid UserId,
    DateTimeOffset OccurredAt) : IUsersEvent;
