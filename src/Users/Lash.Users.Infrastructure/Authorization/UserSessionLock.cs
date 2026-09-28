using Lash.Users.Application.Abstractions;
using Lash.Users.Infrastructure.DbContexts;
using Microsoft.EntityFrameworkCore;

namespace Lash.Users.Infrastructure.Authorization;

public sealed class UserSessionLock(UsersDbContext dbContext) : IUserSessionLock
{
    public async Task<bool> TryAcquireAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        if (dbContext.Database.CurrentTransaction is null)
        {
            throw new InvalidOperationException("A transaction is required to acquire a user session lock.");
        }

        var users = await dbContext.Users
            .FromSqlInterpolated($"SELECT * FROM users.users WHERE id = {userId} FOR UPDATE")
            .AsNoTracking()
            .ToArrayAsync(cancellationToken);

        return users.Length == 1;
    }
}
