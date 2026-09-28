namespace Lash.Users.Infrastructure.Seeding;

public interface ISeederRegistry
{
    IReadOnlyCollection<ISeeder> All { get; }
}
