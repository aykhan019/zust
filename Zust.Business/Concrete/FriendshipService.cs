using Zust.Business.Abstract;
using Zust.DataAccess.Abstract;
using Zust.Entities.Models;

namespace Zust.Business.Concrete
{
    /// <summary>
    /// Represents a service that handles friendships between users.
    /// </summary>
    public class FriendshipService : IFriendshipService
    {
        /// <summary>
        /// Private field representing the data access layer for managing friendships.
        /// </summary>
        private readonly IFriendshipDal _friendshipDal;

        /// <summary>
        /// Private field representing the service responsible for user-related operations.
        /// </summary>
        private readonly IUserService _userService;

        /// <summary>
        /// Private field for the user data access layer, used to fetch a filtered set of users
        /// (a WHERE ... IN query) instead of loading the entire users table into memory.
        /// </summary>
        private readonly IUserDal _userDal;

        /// <summary>
        /// Initializes a new instance of the FriendshipService class with the specified FriendshipDal and UserService.
        /// </summary>
        /// <param name="friendshipDal">The data access layer for handling friendships.</param>
        /// <param name="userService">The service responsible for user-related operations.</param>
        /// <param name="userDal">The data access layer for fetching users by id.</param>
        public FriendshipService(IFriendshipDal friendshipDal, IUserService userService, IUserDal userDal)
        {
            _friendshipDal = friendshipDal;

            _userService = userService;

            _userDal = userDal;
        }

        /// <summary>
        /// Adds a new friendship asynchronously.
        /// </summary>
        /// <param name="friendship">The Friendship object representing the new friendship to be added.</param>
        /// <returns>Task representing the asynchronous operation.</returns>
        public async Task AddFriendship(Friendship friendship)
        {
            await _friendshipDal.AddAsync(friendship);
        }

        /// <summary>
        /// Retrieves all followers of a user asynchronously.
        /// </summary>
        /// <param name="userId">The ID of the user whose followers are to be retrieved.</param>
        /// <returns>A collection of User objects representing all followers of the user.</returns>
        public async Task<IEnumerable<User?>> GetAllFollowersOfUserAsync(string userId)
        {
            // Filter friendships at the database (WHERE FriendId = userId) rather than loading the
            // whole table and filtering in memory.
            var friendships = await _friendshipDal.GetAllAsync(f => f.FriendId == userId);

            var followerIds = friendships.Select(f => f.UserId).ToHashSet();

            if (followerIds.Count == 0)
            {
                return Enumerable.Empty<User?>();
            }

            // Fetch just the follower rows with a single WHERE ... IN query.
            var followers = await _userDal.GetAllAsync(u => followerIds.Contains(u.Id));

            return followers;
        }

        /// <summary>
        /// Retrieves all followings of a user asynchronously.
        /// </summary>
        /// <param name="userId">The ID of the user whose followings are to be retrieved.</param>
        /// <returns>A collection of User objects representing all followings of the user.</returns>
        public async Task<IEnumerable<User?>> GetAllFollowingsOfUserAsync(string userId)
        {
            // Filter friendships at the database (WHERE UserId = userId).
            var friendships = await _friendshipDal.GetAllAsync(f => f.UserId == userId);

            var followingIds = friendships.Select(f => f.FriendId).ToHashSet();

            if (followingIds.Count == 0)
            {
                return Enumerable.Empty<User?>();
            }

            // Fetch just the following rows with a single WHERE ... IN query.
            var followings = await _userDal.GetAllAsync(u => followingIds.Contains(u.Id));

            return followings;
        }

        /// <summary>
        /// Retrieves a friendship asynchronously based on the user IDs.
        /// </summary>
        /// <param name="userId">The ID of the user.</param>
        /// <param name="friendId">The ID of the friend.</param>
        /// <returns>The Friendship object that matches the given user IDs, or null if not found.</returns>
        public async Task<Friendship?> GetFriendshipAsync(string userId, string friendId)
        {
            var friendship = await _friendshipDal.GetAsync(f => f.UserId == userId && f.FriendId == friendId);

            return friendship;
        }

        /// <summary>
        /// Deletes a friendship asynchronously.
        /// </summary>
        /// <param name="userId">The ID of the user.</param>
        /// <param name="friendId">The ID of the friend.</param>
        /// <returns>True if the friendship is deleted successfully; otherwise, false.</returns>
        public async Task<bool> DeleteFriendshipAsync(string userId, string friendId)
        {
            var friendship = await GetFriendshipAsync(userId, friendId);

            if (friendship != null)
            {
                await _friendshipDal.DeleteAsync(friendship);

                return true;
            }

            return false;
        }

        /// <summary>
        /// Checks if a friendship exists between two users asynchronously.
        /// </summary>
        /// <param name="userId">The ID of the user.</param>
        /// <param name="friendId">The ID of the friend.</param>
        /// <returns>True if a friendship exists between the two users; otherwise, false.</returns>
        public async Task<bool> IsFriendAsync(string userId, string friendId)
        {
            var friendship = await GetFriendshipAsync(userId, friendId);

            return friendship != null;
        }

        /// <summary>
        /// Deletes all friendships associated with a user asynchronously.
        /// </summary>
        /// <param name="userId">The ID of the user whose friendships are to be deleted.</param>
        /// <returns>Task representing the asynchronous operation.</returns>
        public async Task DeleteUserFriendshipsAsync(string userId)
        {
            var friendships = await _friendshipDal.GetAllAsync(f => f.UserId == userId || f.FriendId == userId);

            if (friendships != null)
            {
                foreach (var friendship in friendships)
                {
                    await _friendshipDal.DeleteAsync(friendship);
                }
            }
        }
    }
}
