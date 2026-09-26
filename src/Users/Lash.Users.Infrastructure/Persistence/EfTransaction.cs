using Microsoft.EntityFrameworkCore.Storage;
using WebFlow.Abstractions.Interfaces;

namespace Lash.Users.Infrastructure.Persistence;

internal sealed class EfTransaction(IDbContextTransaction transaction) : ITransaction
{
    private bool _completed;

    public async Task CommitAsync(CancellationToken cancellationToken = default)
    {
        await transaction.CommitAsync(cancellationToken);
        _completed = true;
    }

    public async Task RollbackAsync(CancellationToken cancellationToken = default)
    {
        await transaction.RollbackAsync(cancellationToken);
        _completed = true;
    }

    public async ValueTask DisposeAsync()
    {
        if (!_completed)
        {
            await transaction.RollbackAsync();
        }

        await transaction.DisposeAsync();
    }
}
