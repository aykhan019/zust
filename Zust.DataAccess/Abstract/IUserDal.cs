using Zust.Core.Concrete;
using Zust.Entities.Models;

namespace Zust.DataAccess.Abstract
{
    /// <summary>
    /// Represents a data access layer for the User entity.
    /// </summary>
    public interface IUserDal : IEntityRepository<User>
    {
        /// <summary>
        /// Retrieves a page of users excluding the given user, ordered by username and paginated
        /// at the database (OFFSET/LIMIT) so the whole users table is never loaded into memory.
        /// </summary>
        /// <param name="excludeUserId">The id of the user to exclude (typically the current user).</param>
        /// <param name="skip">Number of users to skip.</param>
        /// <param name="take">Maximum number of users to return.</param>
        Task<IEnumerable<User>> GetUsersOtherThanAsync(string excludeUserId, int skip, int take);

        /// <summary>
        /// Retrieves a set of random users, selected at the database (ORDER BY RANDOM()).
        /// </summary>
        /// <param name="count">Maximum number of random users to return.</param>
        Task<IEnumerable<User>> GetRandomUsersAsync(int count);
    }
}
