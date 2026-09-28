using Lash.Users.Infrastructure.Seeding;
using Microsoft.Extensions.Logging.Abstractions;

namespace Lash.Users.UnitTests.Infrastructure.Seeding;

public sealed class DatabaseSeederTests
{
    [Fact]
    public async Task SeedAsync_WhenSeedersHaveDifferentOrder_RunsThemInOrder()
    {
        var executionOrder = new List<int>();
        var seeder = new DatabaseSeeder(
        new TestSeederRegistry(
        [
            new TestSeeder(300, executionOrder),
            new TestSeeder(100, executionOrder),
            new TestSeeder(200, executionOrder)
        ]),
        NullLogger<DatabaseSeeder>.Instance);

        await seeder.SeedAsync();

        Assert.Equal([100, 200, 300], executionOrder);
    }

    private sealed class TestSeederRegistry(IReadOnlyCollection<ISeeder> all) : ISeederRegistry
    {
        public IReadOnlyCollection<ISeeder> All => all;
    }

    private sealed class TestSeeder(int order, ICollection<int> executionOrder) : ISeeder
    {
        public int Order => order;

        public Task SeedAsync(CancellationToken cancellationToken = default)
        {
            executionOrder.Add(Order);
            return Task.CompletedTask;
        }
    }
}
