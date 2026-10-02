using EventTicketing.Core.DataAccess;
using EventTicketing.Domain.Entities;

namespace EventTicketing.DataAccess.Abstract;

public interface IUserDal : IEntityRepository<AppUser>
{
    Task<AppUser?> GetByEmailAsync(string email, CancellationToken ct = default);

    Task<bool> EmailExistsAsync(string email, CancellationToken ct = default);
}
