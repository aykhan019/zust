using Zust.Business.Abstract;
using Zust.DataAccess.Abstract;
using Zust.Entities.Models;

namespace Zust.Business.Concrete
{
    /// <summary>
    /// Represents a service that handles likes.
    /// </summary>
    public class LikeService : ILikeService
    {
        /// <summary>
        /// Private field representing the data access layer for managing likes.
        /// </summary>
        private readonly ILikeDal _likeDal;

        /// <summary>
        /// Initializes a new instance of the LikeService class with the specified LikeDal.
        /// </summary>
        /// <param name="likeDal">The data access layer for handling likes.</param>
        public LikeService(ILikeDal likeDal)
        {
            _likeDal = likeDal;
        }

        // Add more methods and logic here for managing likes.

        /// <summary>
        /// Adds a like to a post asynchronously.
        /// </summary>
        /// <param name="like">The Like object representing the like to be added.</param>
        /// <returns>Task representing the asynchronous operation.</returns>
        public async Task AddLikeToPostAsync(Like like)
        {
            await _likeDal.AddAsync(like);
        }

        /// <summary>
        /// Deletes a like asynchronously.
        /// </summary>
        /// <param name="like">The Like object representing the like to be deleted.</param>
        /// <returns>Task representing the asynchronous operation.</returns>
        public async Task DeleteLikeAsync(Like like)
        {
            await _likeDal.DeleteAsync(like);
        }

        /// <summary>
        /// Deletes all likes associated with a user asynchronously.
        /// </summary>
        /// <param name="userId">The ID of the user whose likes are to be deleted.</param>
        /// <returns>Task representing the asynchronous operation.</returns>
        public async Task DeleteUserLikesAsync(string userId)
        {
            var usersLikes = await _likeDal.GetAllAsync(l => l.UserId == userId);

            if (usersLikes != null)
            {
                foreach (var userLike in usersLikes)
                {
                    await RemoveLikeAsync(userLike);
                }
            }
        }

        /// <summary>
        /// Retrieves the post IDs that a user has liked asynchronously.
        /// </summary>
        /// <param name="userId">The ID of the user.</param>
        /// <returns>A collection of strings representing the post IDs liked by the user.</returns>
        public async Task<IEnumerable<string?>> GetPostIdsUserLikedAsync(string userId)
        {
            return (await _likeDal.GetAllAsync(l => l.UserId == userId)).Select(e => e.PostId);
        }

        /// <summary>
        /// Retrieves the number of likes for a post asynchronously.
        /// </summary>
        /// <param name="postId">The ID of the post.</param>
        /// <returns>The number of likes for the post.</returns>
        public async Task<int> GetPostLikeCountAsync(string postId)
        {
            return (await _likeDal.GetAllAsync(l => l.PostId == postId)).Count();
        }

        /// <summary>
        /// Retrieves the total like count across a set of posts in a single query.
        /// </summary>
        /// <param name="postIds">The IDs of the posts whose likes will be counted.</param>
        /// <returns>The combined number of likes across all the specified posts.</returns>
        public async Task<int> GetTotalLikeCountForPostsAsync(IEnumerable<string> postIds)
        {
            var ids = postIds as ICollection<string> ?? postIds.ToList();

            if (ids.Count == 0)
            {
                return 0;
            }

            // Single WHERE PostId IN (...) query instead of one query per post.
            return (await _likeDal.GetAllAsync(l => ids.Contains(l.PostId))).Count();
        }

        /// <summary>
        /// Retrieves all likes for a post asynchronously.
        /// </summary>
        /// <param name="postId">The ID of the post.</param>
        /// <returns>A collection of Like objects representing the likes for the post.</returns>
        public async Task<IEnumerable<Like>> GetPostLikesAsync(string postId)
        {
            return await _likeDal.GetAllAsync(l => l.PostId == postId);
        }

        /// <summary>
        /// Removes a like asynchronously.
        /// </summary>
        /// <param name="like">The Like object representing the like to be removed.</param>
        /// <returns>Task representing the asynchronous operation.</returns>
        public async Task RemoveLikeAsync(Like like)
        {
            await _likeDal.DeleteAsync(like);
        }

        /// <summary>
        /// Removes a like asynchronously based on the user ID and post ID.
        /// </summary>
        /// <param name="userId">The ID of the user.</param>
        /// <param name="postId">The ID of the post.</param>
        /// <returns>Task representing the asynchronous operation.</returns>
        public async Task RemoveLikeAsync(string userId, string postId)
        {
            var like = await _likeDal.GetAsync(l => l.UserId == userId && l.PostId == postId);
            await RemoveLikeAsync(like);
        }

        /// <summary>
        /// Checks if a user has liked a post asynchronously.
        /// </summary>
        /// <param name="userId">The ID of the user.</param>
        /// <param name="postId">The ID of the post.</param>
        /// <returns>True if the user has liked the post; otherwise, false.</returns>
        public async Task<bool> UserLikedPostAsync(string userId, string postId)
        {
            var result = await _likeDal.GetAsync(l => l.UserId == userId && l.PostId == postId);
            return result != null;
        }
    }
}
