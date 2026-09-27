namespace Lash.Users.UnitTests;

internal static class FixedTimeProvider
{
    public static global::System.TimeProvider System { get; } = new FixedClock();

    private sealed class FixedClock : global::System.TimeProvider
    {
        public override DateTimeOffset GetUtcNow() => new(2026, 1, 1, 0, 0, 0, TimeSpan.Zero);
    }
}
