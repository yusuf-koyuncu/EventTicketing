using System.Data;
using EventTicketing.Core.DataAccess;
using EventTicketing.DataAccess.Concrete.EntityFramework.Contexts;
using Microsoft.EntityFrameworkCore;

namespace EventTicketing.DataAccess.Concrete.EntityFramework;

public class EfUnitOfWork : IUnitOfWork
{
    private readonly EventTicketingDbContext _context;

    public EfUnitOfWork(EventTicketingDbContext context)
    {
        _context = context;
    }

    public async Task<TResult> ExecuteInTransactionAsync<TResult>(
        Func<CancellationToken, Task<TResult>> operation, CancellationToken ct = default)
    {
        var strategy = _context.Database.CreateExecutionStrategy();

        return await strategy.ExecuteAsync(async () =>
        {
            await using var transaction = await _context.Database
                .BeginTransactionAsync(IsolationLevel.ReadCommitted, ct);

            var result = await operation(ct);

            await transaction.CommitAsync(ct);
            return result;
        });
    }

    public Task<int> SaveChangesAsync(CancellationToken ct = default) =>
        _context.SaveChangesAsync(ct);
}
