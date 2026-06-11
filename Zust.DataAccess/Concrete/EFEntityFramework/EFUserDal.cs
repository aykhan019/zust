using Microsoft.EntityFrameworkCore;
using Zust.Core.Concrete.EntityFramework;
using Zust.DataAccess.Abstract;
using Zust.Entities.Models;

namespace Zust.DataAccess.Concrete.EFEntityFramework
{
    /// <summary>
    /// Represents the Entity Framework implementation of the IUserDal interface.
    /// </summary>
    public class EFUserDal : EfEntityRepositoryBase<User, ZustDbContext>, IUserDal
    {
        /// <inheritdoc />
        public async Task<IEnumerable<User>> GetUsersOtherThanAsync(string excludeUserId, int skip, int take)
        {
            using var context = new ZustDbContext();

            return await context.Set<User>()
                                 .AsNoTracking()
                                 .Where(u => u.Id != excludeUserId)
                                 .OrderBy(u => u.UserName)
                                 .Skip(skip)
                                 .Take(take)
                                 .ToListAsync();
        }
    }
}
