namespace Lash.Users.Contracts.Events;

public interface IUsersEvent
{
    Guid EventId { get; }
    Guid UserId { get; }
    DateTimeOffset OccurredAt { get; }
}
