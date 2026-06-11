using Microsoft.AspNetCore.Mvc;
using Zust.Business.Abstract;
using Zust.Entities.Models;
using Zust.Web.Abstract;
using Zust.Web.Helpers.ConstantHelpers;
using Zust.Web.Helpers.Utilities;
using Zust.Web.Models;

namespace Zust.Web.Controllers.ApiControllers
{
    /// <summary>
    /// API controller responsible for handling posts and related operations.
    /// </summary>
    [Route(Routes.PostAPI)]
    [Controller]
    public class PostController : ControllerBase
    {
        /// <summary>
        /// The service responsible for handling media-related operations.
        /// </summary>
        private readonly IMediaService _mediaService;

        /// <summary>
        /// The service responsible for handling post-related operations.
        /// </summary>
        private readonly IPostService _postService;

        /// <summary>
        /// The service responsible for handling user-related operations.
        /// </summary>
        private readonly IUserService _userService;

        /// <summary>
        /// The service responsible for handling comment-related operations.
        /// </summary>
        private readonly ICommentService _commentService;

        /// <summary>
        /// The service responsible for handling notification-related operations.
        /// </summary>
        private readonly INotificationService _notificationService;

        /// <summary>
        /// The service responsible for handling friendship-related operations.
        /// </summary>
        private readonly IFriendshipService _friendshipService;

        /// <summary>
        /// Initializes a new instance of the PostController class with the specified services.
        /// </summary>
        /// <param name="mediaService">The service for handling media-related operations.</param>
        /// <param name="postService">The service for handling post-related operations.</param>
        /// <param name="userService">The service for handling user-related operations.</param>
        /// <param name="commentService">The service for handling comment-related operations.</param>
        /// <param name="notificationService">The service for handling notification-related operations.</param>
        /// <param name="friendshipService">The service for handling friendship-related operations.</param>
        public PostController(IMediaService mediaService, IPostService postService, IUserService userService, ICommentService commentService, INotificationService notificationService, IFriendshipService friendshipService)
        {
            _mediaService = mediaService;

            _postService = postService;

            _userService = userService;

            _commentService = commentService;

            _notificationService = notificationService;

            _friendshipService = friendshipService;
        }

        /// <summary>
        /// Creates a new post and adds it to the database.
        /// </summary>
        /// <param name="model">The view model containing the post details.</param>
        [HttpPost(Routes.CreatePost)]
        public async Task<IActionResult> CreatePost([FromForm] CreatePostViewModel model)
        {
            try
            {
                IFormFile? mediaFile = model.MediaFile;

                var currentUser = await UserHelper.GetCurrentUserAsync(HttpContext);

                var post = new Post()
                {
                    Id = Guid.NewGuid().ToString(),

                    CreatedAt = DateTime.Now,

                    Description = model.Description,

                    UserId = currentUser.Id
                };

                // If mediaFile is null, it mean post does not have an image
                if (mediaFile == null)
                {
                    post.ContentUrl = Constants.NoContentImageUrl;

                    post.HasMediaContent = false;

                    post.IsVideo = false;

                    await _postService.AddPostAsync(post);
                }
                else
                {
                    var mediaUrl = await _mediaService.UploadMediaAsync(mediaFile);

                    if (mediaUrl != string.Empty)
                    {
                        var isVideoFile = _mediaService.IsVideoFile(mediaFile);

                        post.ContentUrl = mediaUrl;

                        post.HasMediaContent = true;

                        post.IsVideo = isVideoFile;

                        await _postService.AddPostAsync(post);
                    }
                    else
                    {
                        return BadRequest(Errors.ImageUploadError);
                    }
                }
                post.User = await _userService.GetUserByIdAsync(post.UserId);

                // Send notification to all followers
                var followers = await _friendshipService.GetAllFollowersOfUserAsync(currentUser.Id);

                foreach (var follower in followers)
                {
                    var notification = new Notification()
                    {
                        Id = Guid.NewGuid().ToString(),

                        Date = DateTime.Now,

                        FromUserId = currentUser.Id,

                        ToUserId = follower.Id,

                        IsRead = false,

                        Message = NotificationType.GetSharedPostMessage(currentUser.UserName)
                    };

                    await _notificationService.AddAsync(notification);
                }

                return Ok(post);

            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        /// <summary>
        /// Gets all posts for the news feed of the current user.
        /// </summary>
        [HttpGet(Routes.GetAllPosts)]
        public async Task<ActionResult<IEnumerable<User>>> GetAllPosts()
        {
            try
            {
                var currentUser = await UserHelper.GetCurrentUserAsync(HttpContext);

                var posts = await _postService.GetAllPostsForNewsFeedAsync(currentUser.Id);

                // Project to only the fields the feed actually renders. Returning the full Post
                // (with its entire Identity User embedded per post) produced a ~24 MB response and
                // leaked sensitive columns (PasswordHash, SecurityStamp, ...). Property names are
                // camelCase to match what the client reads (post.user.imageUrl, etc.).
                var feed = posts.Select(p => new
                {
                    id = p.Id,
                    description = p.Description,
                    hasMediaContent = p.HasMediaContent,
                    contentUrl = p.ContentUrl,
                    isVideo = p.IsVideo,
                    createdAt = p.CreatedAt,
                    userId = p.UserId,
                    user = p.User == null ? null : new
                    {
                        id = p.User.Id,
                        userName = p.User.UserName,
                        imageUrl = p.User.ImageUrl
                    }
                });

                return Ok(feed);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        /// <summary>
        /// Retrieves all posts of a specific user by their user ID.
        /// </summary>
        /// <param name="userId">The ID of the user whose posts to retrieve.</param>
        /// <returns>An action result containing the list of posts belonging to the user.</returns>
        [HttpGet(Routes.GetAllPostsOfUser)]
        public async Task<ActionResult<IEnumerable<User>>> GetAllPostsOfUser(string userId)
        {
            try
            {
                var posts = await _postService.GetAllPostsOfUserAsync(userId);

                return Ok(posts);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        /// <summary>
        /// Retrieves the total number of likes on all posts of a specific user by their user ID.
        /// </summary>
        /// <param name="userId">The ID of the user whose posts' like count to retrieve.</param>
        /// <returns>An action result containing the total number of likes on the user's posts.</returns>
        [HttpGet(Routes.GetAllPostsLikeCount)]
        public async Task<ActionResult<int>> GetAllPostsLikeCount(string userId)
        {
            try
            {
                var postLikeCount = await _postService.GetAllPostsLikeCountAsync(userId);

                return Ok(postLikeCount);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        /// <summary>
        /// Retrieves all comments of a specific post by its post ID.
        /// </summary>
        /// <param name="postId">The ID of the post whose comments to retrieve.</param>
        /// <returns>An action result containing the list of comments belonging to the post.</returns>
        [HttpGet(Routes.GetCommentsOfPost)]
        public async Task<ActionResult<IEnumerable<Comment>>> GetAllCommentsOfPost(string postId)
        {
            try
            {
                var comments = (await _commentService.GetCommentsOfPostAsync(postId)).ToList();

                // Sequential await on the shared DbContext (a `ForEach(async ...)` here ran these
                // as fire-and-forget tasks that raced the DbContext and could crash the process).
                foreach (var comment in comments)
                {
                    comment.User = await _userService.GetUserByIdAsync(comment.UserId);
                }

                return Ok(comments);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        /// <summary>
        /// Retrieves the total count of comments on a specific post by its post ID.
        /// </summary>
        /// <param name="postId">The ID of the post whose comments' count to retrieve.</param>
        /// <returns>An action result containing the total count of comments on the post.</returns>
        [HttpGet(Routes.GetCountOfCommentsOfPost)]
        public async Task<ActionResult<int>> GetCountOfCommentsOfPost(string postId)
        {
            try
            {
                var comments = await _commentService.GetCommentsOfPostAsync(postId);

                return Ok(comments.Count());
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        /// <summary>
        /// Adds a new comment to a specific post by its post ID.
        /// </summary>
        /// <param name="model">The input model containing comment details.</param>
        /// <returns>An action result containing the newly added comment and related notification.</returns>
        [HttpPost(Routes.AddComment)]
        public async Task<ActionResult<Comment>> AddComment([FromBody] CommentInputModel model)
        {
            try
            {
                var post = await _postService.GetPostByIdAsync(model.PostId);

                if (post == null)
                {
                    return NotFound();
                }

                var comment = new Comment
                {
                    Id = Guid.NewGuid().ToString(),

                    Text = model.Text,

                    UserId = model.UserId,

                    PostId = model.PostId
                };

                await _commentService.AddAsync(comment);

                var currentUser = await _userService.GetUserByIdAsync(model.UserId);

                var notification = new Notification()
                {
                    Id = Guid.NewGuid().ToString(),

                    Date = DateTime.Now,

                    IsRead = false,

                    FromUserId = currentUser.Id,

                    FromUser = currentUser,

                    ToUserId = post.UserId,

                    ToUser = await _userService.GetUserByIdAsync(post.UserId),

                    Message = NotificationType.GetCommentedOnYourPostMessage(currentUser.UserName),
                };

                await _notificationService.AddAsync(notification);

                comment.User = await _userService.GetUserByIdAsync(model.UserId);

                var vm = new CommentNotificationViewModel()
                {
                    Notification = notification,

                    Comment = comment
                };

                return Ok(vm);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }
    }
}
