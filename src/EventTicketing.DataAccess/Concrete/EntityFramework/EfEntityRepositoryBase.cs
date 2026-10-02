using System.Linq.Expressions;
using EventTicketing.Core.DataAccess;
using EventTicketing.Core.Entities;
using Microsoft.EntityFrameworkCore;

namespace EventTicketing.DataAccess.Concrete.EntityFramework;

public class EfEntityRepositoryBase<TEntity, TContext> : IEntityRepository<TEntity>
    where TEntity : class, IEntity
    where TContext : DbContext
{
    protected readonly TContext Context;

    public EfEntityRepositoryBase(TContext context)
    {
        Context = context;
    }

    public async Task<TEntity?> GetAsync(
        Expression<Func<TEntity, bool>> filter, CancellationToken ct = default) =>
        await Context.Set<TEntity>().SingleOrDefaultAsync(filter, ct);

    public async Task AddAsync(TEntity entity, CancellationToken ct = default) =>
        await Context.Set<TEntity>().AddAsync(entity, ct);
}
