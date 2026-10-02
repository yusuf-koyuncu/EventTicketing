using EventTicketing.DataAccess.Abstract;
using EventTicketing.DataAccess.Concrete.EntityFramework.Contexts;
using EventTicketing.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace EventTicketing.DataAccess.Concrete.EntityFramework;

public class EfUserDal : EfEntityRepositoryBase<AppUser, EventTicketingDbContext>, IUserDal
{
    public EfUserDal(EventTicketingDbContext context) : base(context)
    {
    }

    public async Task<AppUser?> GetByEmailAsync(string email, CancellationToken ct = default) =>
        await Context.Users.SingleOrDefaultAsync(u => u.Email == email, ct);

    public async Task<bool> EmailExistsAsync(string email, CancellationToken ct = default) =>
        await Context.Users.AnyAsync(u => u.Email == email, ct);
}
