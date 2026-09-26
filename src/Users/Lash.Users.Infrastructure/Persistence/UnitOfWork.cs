using System.Data;
using Lash.Users.Infrastructure.DbContexts;
using Microsoft.EntityFrameworkCore;
using WebFlow.Abstractions.Interfaces;

namespace Lash.Users.Infrastructure.Persistence;

public sealed class UnitOfWork(UsersDbContext dbContext) : IUnitOfWork
{
    public async Task<ITransaction> BeginTransactionAsync(
        CancellationToken cancellationToken = default) =>
        new EfTransaction(await dbContext.Database.BeginTransactionAsync(cancellationToken));

    public async Task<ITransaction> BeginTransactionAsync(
        IsolationLevel isolationLevel,
        CancellationToken cancellationToken = default) =>
        new EfTransaction(await dbContext.Database.BeginTransactionAsync(isolationLevel, cancellationToken));

    public async Task SaveChangesAsync(CancellationToken cancellationToken = default) =>
        await dbContext.SaveChangesAsync(cancellationToken);
}
