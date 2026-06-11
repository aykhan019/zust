using Microsoft.EntityFrameworkCore;
using Zust.Core.Concrete.EntityFramework;
using Zust.DataAccess.Abstract;
using Zust.Entities.Models;

namespace Zust.DataAccess.Concrete.EFEntityFramework
{
    /// <summary>
    /// Represents the Entity Framework implementation of the IPostDal interface.
    /// </summary>
    public class EFPostDal : EfEntityRepositoryBase<Post, ZustDbContext>, IPostDal
    {
        /// <inheritdoc />
        public async Task<IEnumerable<Post>> GetRecentForFeedAsync(string excludeUserId, int take)
        {
            using var context = new ZustDbContext();

            return await context.Set<Post>()
                                 .AsNoTracking()
                                 .Where(p => p.UserId != excludeUserId)
                                 .OrderByDescending(p => p.CreatedAt)
                                 .Take(take)
                                 .ToListAsync();
        }
    }
}
