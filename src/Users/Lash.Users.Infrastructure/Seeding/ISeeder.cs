namespace Lash.Users.Infrastructure.Seeding;

public interface ISeeder
{
    int Order { get; }

    Task SeedAsync(CancellationToken cancellationToken = default);
}
