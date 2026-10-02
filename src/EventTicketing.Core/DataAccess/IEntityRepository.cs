using System.Linq.Expressions;
using EventTicketing.Core.Entities;

namespace EventTicketing.Core.DataAccess;

public interface IEntityRepository<TEntity> where TEntity : class, IEntity
{
    Task<TEntity?> GetAsync(Expression<Func<TEntity, bool>> filter, CancellationToken ct = default);

    Task AddAsync(TEntity entity, CancellationToken ct = default);
}
