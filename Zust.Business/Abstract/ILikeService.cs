using Zust.Entities.Models;

namespace Zust.Business.Abstract
{
    /// <summary>
    /// Interface for managing like-related operations, such as adding and removing likes to/from posts,
    /// retrieving post like count, checking if a user liked a post, and deleting user likes.
    /// </summary>
    public interface ILikeService
    {
        /// <summary>
        /// Retrieves the like count of a post asynchronously.
        /// </summary>
        /// <param name="postId">The ID of the post whose like count will be retrieved.</param>
        /// <returns>The number of likes for the specified post.</returns>
        Task<int> GetPostLikeCountAsync(string postId);

        /// <summary>
        /// Retrieves the total like count across a set of posts in a single query.
        /// </summary>
        /// <param name="postIds">The IDs of the posts whose likes will be counted.</param>
        /// <returns>The combined number of likes across all the specified posts.</returns>
        Task<int> GetTotalLikeCountForPostsAsync(IEnumerable<string> postIds);

        /// <summary>
        /// Adds a new like to a post asynchronously.
        /// </summary>
        /// <param name="like">The like object to be added.</param>
        /// <returns>Task representing the asynchronous operation.</returns>
        Task AddLikeToPostAsync(Like like);

        /// <summary>
        /// Retrieves the IDs of the posts that a user liked asynchronously.
        /// </summary>
        /// <param name="userId">The ID of the user whose liked posts will be retrieved.</param>
        /// <returns>A collection of strings representing the IDs of the posts that the user liked.</returns>
        Task<IEnumerable<string>> GetPostIdsUserLikedAsync(string userId);

        /// <summary>
        /// Removes a like from a post asynchronously.
        /// </summary>
        /// <param name="like">The like object to be removed.</param>
        /// <returns>Task representing the asynchronous operation.</returns>
        Task RemoveLikeAsync(Like like);

        /// <summary>
        /// Removes a like from a post asynchronously based on the user ID and post ID.
        /// </summary>
        /// <param name="userId">The ID of the user who liked the post.</param>
        /// <param name="postId">The ID of the post from which the like will be removed.</param>
        /// <returns>Task representing the asynchronous operation.</returns>
        Task RemoveLikeAsync(string userId, string postId);

        /// <summary>
        /// Checks if a user liked a post asynchronously.
        /// </summary>
        /// <param name="userId">The ID of the user whose like will be checked.</param>
        /// <param name="postId">The ID of the post to be checked for a like.</param>
        /// <returns>True if the user liked the post; otherwise, false.</returns>
        Task<bool> UserLikedPostAsync(string userId, string postId);

        /// <summary>
        /// Deletes all likes of a user asynchronously.
        /// </summary>
        /// <param name="userId">The ID of the user whose likes will be deleted.</param>
        /// <returns>Task representing the asynchronous operation.</returns>
        Task DeleteUserLikesAsync(string userId);

        /// <summary>
        /// Retrieves all likes of a post asynchronously.
        /// </summary>
        /// <param name="postId">The ID of the post whose likes will be retrieved.</param>
        /// <returns>A collection of Like objects representing the likes of the post.</returns>
        Task<IEnumerable<Like>> GetPostLikesAsync(string postId);

        /// <summary>
        /// Deletes a like asynchronously.
        /// </summary>
        /// <param name="like">The like object to be deleted.</param>
        /// <returns>Task representing the asynchronous operation.</returns>
        Task DeleteLikeAsync(Like like);
    }
}
