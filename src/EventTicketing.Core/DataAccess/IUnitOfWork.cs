namespace EventTicketing.Core.DataAccess;

public interface IUnitOfWork
{
    Task<TResult> ExecuteInTransactionAsync<TResult>(
        Func<CancellationToken, Task<TResult>> operation, CancellationToken ct = default);

    Task<int> SaveChangesAsync(CancellationToken ct = default);
}
