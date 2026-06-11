using Zust.Core.Concrete;
using Zust.Entities.Models;

namespace Zust.DataAccess.Abstract
{
    /// <summary>
    /// Represents a data access layer for the Post entity.
    /// </summary>
    public interface IPostDal : IEntityRepository<Post>
    {
        /// <summary>
        /// Retrieves the most recent posts for the news feed, excluding the given user, ordered
        /// newest-first and limited at the database. Avoids loading the entire posts table.
        /// </summary>
        /// <param name="excludeUserId">The id of the user whose own posts are excluded.</param>
        /// <param name="take">The maximum number of posts to return.</param>
        Task<IEnumerable<Post>> GetRecentForFeedAsync(string excludeUserId, int take);
    }
}
