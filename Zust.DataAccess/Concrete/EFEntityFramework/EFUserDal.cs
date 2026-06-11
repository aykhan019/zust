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

        /// <inheritdoc />
        public async Task<IEnumerable<User>> SearchUsersByNameAsync(string excludeUserId, string text)
        {
            using var context = new ZustDbContext();

            var pattern = $"%{text}%";

            // Filter and order at the database so the whole users table is never materialised.
            // EF.Functions.Like keeps the match case-insensitive without pulling rows into memory.
            return await context.Set<User>()
                                 .AsNoTracking()
                                 .Where(u => u.Id != excludeUserId && EF.Functions.Like(u.UserName, pattern))
                                 .OrderBy(u => u.UserName)
                                 .ToListAsync();
        }

        /// <inheritdoc />
        public async Task<IEnumerable<User>> GetRandomUsersAsync(int count)
        {
            using var context = new ZustDbContext();

            // Pick `count` random users at the database (ORDER BY RANDOM()) so the whole users
            // table is never loaded into memory.
            return await context.Set<User>()
                                 .AsNoTracking()
                                 .OrderBy(u => EF.Functions.Random())
                                 .Take(count)
                                 .ToListAsync();
        }
    }
}
