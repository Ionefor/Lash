namespace Lash.Users.Messaging.Events;

public interface IUsersEvent
{
    Guid EventId { get; }
    Guid UserId { get; }
    DateTimeOffset OccurredAt { get; }
}
