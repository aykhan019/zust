using AutoMapper;
using Microsoft.AspNetCore.Mvc;
using Zust.Business.Abstract;
using Zust.Entities.Models;
using Zust.Web.Abstract;
using Zust.Web.DTOs;
using Zust.Web.Extensions;
using Zust.Web.Helpers.ConstantHelpers;
using Zust.Web.Helpers.Utilities;
using Zust.Web.Models;

namespace Zust.Web.Controllers.ApiControllers
{
    /// <summary>
    /// Controller for managing users.
    /// </summary>
    [Route(Routes.UserController)]
    [ApiController]
    public class UserController : ControllerBase
    {
        /// <summary>
        /// The user service used for user-related operations.
        /// </summary>
        private readonly IUserService _userService;

        /// <summary>
        /// The friendship service used for friendship-related operations.
        /// </summary>
        private readonly IFriendshipService _friendshipService;

        /// <summary>
        /// The friend request service used for friend request-related operations.
        /// </summary>
        private readonly IFriendRequestService _friendRequestService;

        /// <summary>
        /// The media service used for media-related operations.
        /// </summary>
        private readonly IMediaService _mediaService;

        /// <summary>
        /// The mapper used for object mapping.
        /// </summary>
        private readonly IMapper _mapper;

        /// <summary>
        /// Initializes a new instance of the UserController class.
        /// </summary>
        /// <param name="userService">The user service used for user-related operations.</param>
        /// <param name="friendshipService">The friendship service used for friendship-related operations.</param>
        /// <param name="friendRequestService">The friend request service used for friend request-related operations.</param>
        /// <param name="mediaService">The media service used for media-related operations.</param>
        /// <param name="mapper">The mapper used for object mapping.</param>
        public UserController(IUserService userService,
                              IFriendshipService friendshipService,
                              IFriendRequestService friendRequestService,
                              IMediaService mediaService,
                              IMapper mapper)
        {
            _userService = userService;

            _friendshipService = friendshipService;

            _friendRequestService = friendRequestService;

            _mediaService = mediaService;

            _mapper = mapper;
        }

        /// <summary>
        /// Retrieves the count of all users.
        /// </summary>
        /// <returns>The count of all users.</returns>
        [HttpGet(Routes.GetAllUsersCount)]
        public async Task<ActionResult<int>> GetAllUsersCount()
        {
            try
            {
                var users = await _userService.GetAllUsersAsync();

                return Ok(users.Count());
            }
            catch
            {
                return Ok(0);
            }
        }

        /// <summary>
        /// Retrieves a list of users within the specified range.
        /// </summary>
        /// <param name="startIndex">The start index of the user range.</param>
        /// <param name="userCount">The number of users to retrieve.</param>
        /// <returns>A list of users within the specified range.</returns>
        [HttpGet(Routes.GetUsers)]
        public async Task<ActionResult<IEnumerable<User>>> GetUsers(int startIndex, int userCount)
        {
            try
            {
                var currentUser = await UserHelper.GetCurrentUserAsync(HttpContext);

                if (currentUser == null)
                {
                    return NotFound(Errors.UserNotFound);
                }

                var currentUserId = currentUser.Id;

                var users = await _userService.GetAllUsersOtherThanAsync(currentUserId);

                var userDTOs = _mapper.Map<List<UserDTO>>(users);

                // Sequential await: the scoped DbContext is not thread-safe, so these calls must
                // not overlap. (Previously `ForEach(async ...)` ran them as fire-and-forget
                // async-void tasks, which both raced the DbContext and crashed the process on error.)
                foreach (var user in userDTOs)
                {
                    user.IsFriend = await _friendshipService.IsFriendAsync(currentUserId, user.Id);
                    if (!user.IsFriend)
                    {
                        user.HasFriendRequestPending = await _friendRequestService.HasRequestPendingAsync(currentUserId, user.Id, Status.Pending);
                    }
                }

                var range = new Range(startIndex, startIndex + userCount);

                return Ok(userDTOs.Take(range));
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        /// <summary>
        /// Retrieves a user by ID.
        /// </summary>
        /// <param name="id">The ID of the user to retrieve.</param>
        /// <returns>The user with the specified ID.</returns>
        [HttpGet(Routes.GetUser)]
        public async Task<ActionResult<IEnumerable<UserDTO>>> GetUser(string id)
        {
            try
            {
                var currentUser = await UserHelper.GetCurrentUserAsync(HttpContext);

                if (currentUser == null)
                {
                    return NotFound(Errors.UserNotFound);
                }

                var user = await _userService.GetUserByIdAsync(id);

                if (user == null)
                {
                    return NotFound(Errors.UserNotFound);
                }

                var userDTO = _mapper.Map<UserDTO>(user);

                userDTO.IsFriend = await _friendshipService.IsFriendAsync(currentUser.Id, user.Id);

                if (!userDTO.IsFriend)
                {
                    userDTO.HasFriendRequestPending = await _friendRequestService.HasRequestPendingAsync(currentUser.Id, user.Id, Status.Pending);
                }

                return Ok(userDTO);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        /// <summary>
        /// Retrieves a list of user profiles that match the specified text.
        /// </summary>
        /// <param name="text">The text to search for in the user profiles.</param>
        /// <returns>A list of user profiles that match the specified text.</returns>
        [HttpGet(Routes.GetUsersByText)]
        public async Task<ActionResult<IEnumerable<UserDTO>>> GetUsersByText(string text)
        {
            try
            {
                var currentUser = await UserHelper.GetCurrentUserAsync(HttpContext);

                if (currentUser == null)
                {
                    return NotFound(Errors.UserNotFound);
                }

                var users = await _userService.GetAllUsersOtherThanAsync(currentUser.Id);

                var filteredUsers = users.Where(u => u.UserName.ToLower().Contains(text.ToLower()));

                var userDTOs = _mapper.Map<List<UserDTO>>(filteredUsers);

                // Sequential await on the shared DbContext (see GetUsers above for rationale).
                foreach (var user in userDTOs)
                {
                    user.IsFriend = await _friendshipService.IsFriendAsync(currentUser.Id, user.Id);

                    if (!user.IsFriend)
                    {
                        user.HasFriendRequestPending = await _friendRequestService.HasRequestPendingAsync(currentUser.Id, user.Id, Status.Pending);
                    }
                }

                return Ok(userDTOs);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        /// <summary>
        /// Retrieves a list of followers for the specified user.
        /// </summary>
        /// <param name="userId">The ID of the user whose followers are to be retrieved.</param>
        /// <returns>A list of followers for the specified user.</returns>
        [HttpGet(Routes.GetFollowers)]
        public async Task<ActionResult<IEnumerable<User>>> GetFollowers(string userId)
        {
            try
            {
                var followers = await _friendshipService.GetAllFollowersOfUserAsync(userId);

                return Ok(followers.ToList());
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        /// <summary>
        /// Retrieves a range of followers for the current user.
        /// </summary>
        /// <param name="startIndex">The start index of the range.</param>
        /// <param name="takeCount">The number of followers to take from the range.</param>
        /// <returns>A range of followers for the current user.</returns>
        [HttpGet(Routes.GetFollowersInRange)]
        public async Task<ActionResult<IEnumerable<User>>> GetFollowersInRange(int startIndex, int takeCount)
        {
            try
            {
                var currentUser = await UserHelper.GetCurrentUserAsync(HttpContext);

                if (currentUser == null)
                {
                    return NotFound(Errors.UserNotFound);
                }

                var currentUserId = currentUser.Id;

                var followers = await _friendshipService.GetAllFollowersOfUserAsync(currentUserId);

                var userDTOs = _mapper.Map<List<UserDTO>>(followers);

                var range = new Range(startIndex, startIndex + takeCount);

                return Ok(followers.Take(range));
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        /// <summary>
        /// Retrieves a list of random followers for the specified user.
        /// </summary>
        /// <param name="userId">The ID of the user whose random followers are to be retrieved.</param>
        /// <returns>A list of random followers for the specified user.</returns>
        [HttpGet(Routes.GetRandomFollowers)]
        public async Task<ActionResult<IEnumerable<User>>> GetRandomFollowers(string userId)
        {
            try
            {
                var followers = await _friendshipService.GetAllFollowersOfUserAsync(userId);

                return Ok(followers.ToList().GetRandomElements(Constants.RandomFollowerCount));
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        /// <summary>
        /// Retrieves the count of followers for the specified user.
        /// </summary>
        /// <param name="userId">The ID of the user whose follower count is to be retrieved.</param>
        /// <returns>The count of followers for the specified user.</returns>
        [HttpGet(Routes.GetFollowersCount)]
        public async Task<ActionResult<int>> GetFollowersCount(string userId)
        {
            try
            {
                var followers = await _friendshipService.GetAllFollowersOfUserAsync(userId);

                var count = followers.Count();

                return Ok(count);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }
        /// <summary>
        /// Retrieves a list of users that the specified user is following.
        /// </summary>
        /// <param name="userId">The ID of the user whose followings are to be retrieved.</param>
        /// <returns>A list of users that the specified user is following.</returns>
        [HttpGet(Routes.GetFollowings)]
        public async Task<ActionResult<IEnumerable<User>>> GetFollowings(string userId)
        {
            try
            {
                var followings = await _friendshipService.GetAllFollowingsOfUserAsync(userId);

                return Ok(followings.ToList());
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        /// <summary>
        /// Retrieves a range of users that the current user is following.
        /// </summary>
        /// <param name="startIndex">The start index of the range.</param>
        /// <param name="takeCount">The number of users to take from the range.</param>
        /// <returns>A range of users that the current user is following.</returns>
        [HttpGet(Routes.GetFollowingsInRange)]
        public async Task<ActionResult<IEnumerable<User>>> GetFollowingsInRange(int startIndex, int takeCount)
        {
            try
            {
                var currentUser = await UserHelper.GetCurrentUserAsync(HttpContext);

                if (currentUser == null)
                {
                    return NotFound(Errors.UserNotFound);
                }

                var currentUserId = currentUser.Id;

                var followings = await _friendshipService.GetAllFollowingsOfUserAsync(currentUserId);

                var userDTOs = _mapper.Map<List<UserDTO>>(followings);

                var range = new Range(startIndex, startIndex + takeCount);

                return Ok(followings.Take(range));
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        /// <summary>
        /// Retrieves the count of users that the specified user is following.
        /// </summary>
        /// <param name="userId">The ID of the user whose following count is to be retrieved.</param>
        /// <returns>The count of users that the specified user is following.</returns>
        [HttpGet(Routes.GetFollowingsCount)]
        public async Task<ActionResult<int>> GetFollowingsCount(string userId)
        {
            try
            {
                var followings = await _friendshipService.GetAllFollowingsOfUserAsync(userId);

                var count = followings.Count();

                return Ok(count);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        /// <summary>
        /// Removes the specified user from the current user's friends list.
        /// </summary>
        /// <param name="friendId">The ID of the user to be removed from the friends list.</param>
        /// <returns>An action result indicating the success or failure of the removal operation.</returns>
        [HttpPost(Routes.RemoveFriend)]
        public async Task<IActionResult> RemoveFriend(string friendId)
        {
            try
            {
                var currentUser = await UserHelper.GetCurrentUserAsync(HttpContext);

                if (currentUser == null)
                {
                    return NotFound(Errors.UserNotFound);
                }

                var currentUserId = currentUser.Id;

                var friendRequest = await _friendRequestService.GetAsync(fr => fr.SenderId == currentUserId && fr.ReceiverId == friendId);

                if (friendRequest != null)
                {
                    await _friendRequestService.DeleteAsync(friendRequest);
                }

                var deleted = await _friendshipService.DeleteFriendshipAsync(currentUserId, friendId);

                if (!deleted)
                {
                    return BadRequest(Errors.FriendRequestNotFound);
                }

                return Ok();
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        /// <summary>
        /// Removes the specified user from the current user's followers list.
        /// </summary>
        /// <param name="friendId">The ID of the user to be removed from the followers list.</param>
        /// <returns>An action result indicating the success or failure of the removal operation.</returns>
        [HttpPost(Routes.RemoveFollower)]
        public async Task<IActionResult> RemoveFollower(string friendId)
        {
            try
            {
                var currentUser = await UserHelper.GetCurrentUserAsync(HttpContext);

                if (currentUser == null)
                {
                    return NotFound(Errors.UserNotFound);
                }

                var currentUserId = currentUser.Id;

                var friendRequest = await _friendRequestService.GetAsync(fr => fr.SenderId == currentUserId && fr.ReceiverId == friendId);

                if (friendRequest != null)
                {
                    await _friendRequestService.DeleteAsync(friendRequest);
                }

                var deleted = await _friendshipService.DeleteFriendshipAsync(friendId, currentUserId);

                if (!deleted)
                {
                    return BadRequest(Errors.FriendRequestNotFound);
                }

                return Ok();
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        /// <summary>
        /// Retrieves the current user's profile.
        /// </summary>
        /// <returns>The profile of the current user.</returns>
        [HttpGet(Routes.GetCurrentUser)]
        public async Task<ActionResult<User?>> GetCurrentUser()
        {
            try
            {
                var user = await UserHelper.GetCurrentUserAsync(HttpContext);

                if (user == null)
                {
                    return NotFound(Errors.UserNotFound);
                }

                return Ok(user);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        /// <summary>
        /// Updates the current user's profile image with the provided media file.
        /// </summary>
        /// <param name="model">The model containing the media file and user ID.</param>
        /// <returns>An action result indicating the success or failure of the update operation.</returns>
        [HttpPost(Routes.UpdateProfileImage)]
        public async Task<IActionResult> UpdateProfileImage([FromForm] UpdateProfileViewModel model)
        {
            try
            {
                var imageFile = model.MediaFile;

                var userId = model.UserId;

                // Check if the file and userId exist and are valid
                if (imageFile != null && imageFile.Length > 0 && !string.IsNullOrEmpty(userId))
                {
                    var mediaUrl = await _mediaService.UploadMediaAsync(imageFile);

                    if (mediaUrl != string.Empty)
                    {
                        var user = await _userService.GetUserByIdAsync(userId);

                        if (user != null)
                        {
                            user.ImageUrl = mediaUrl;

                            await _userService.UpdateAsync(user);

                            return Ok();
                        }

                        return NotFound(Errors.UserNotFound);
                    }

                    return BadRequest(Errors.ImageUploadError);
                }

                return BadRequest(Errors.InvalidRequestData);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        /// <summary>
        /// Retrieves a list of users whose birthday is today.
        /// </summary>
        /// <param name="userId">The ID of the user whose birthday list is to be retrieved.</param>
        /// <returns>A list of users whose birthday is today.</returns>
        [HttpGet(Routes.GetUsersWithTodayBirthday)]
        public async Task<IActionResult> GetUsersWithTodayBirthday(string userId)
        {
            try
            {
                var users = await _userService.GetAllUsersOtherThanAsync(userId);

                var today = DateTime.Today;

                var todayBirthdayUsers = users.Where(user => user.Birthday.Day == today.Day && user.Birthday.Month == today.Month);

                return Ok(todayBirthdayUsers);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        /// <summary>
        /// Retrieves a list of users whose birthday occurred within a recent range.
        /// </summary>
        /// <param name="userId">The ID of the user whose birthday list is to be retrieved.</param>
        /// <returns>A list of users whose birthday occurred within a recent range.</returns>
        [HttpGet(Routes.GetUsersWithRecentBirthday)]
        public async Task<IActionResult> GetUsersWithRecentBirthday(string userId)
        {
            try
            {
                var users = await _userService.GetAllUsersOtherThanAsync(userId);

                var today = DateTime.Today;

                DateTime startDate = today.AddDays(-Constants.BirthdayRange); // 7 days ago (excluding today)

                DateTime endDate = today.AddDays(-1);   // Yesterday (excluding today)

                var birthdayUsersInRange = users.Where(user => user.Birthday >= startDate && user.Birthday <= endDate);

                return Ok(birthdayUsersInRange);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        /// <summary>
        /// Retrieves a list of users whose birthday is coming up in the next few days.
        /// </summary>
        /// <param name="userId">The ID of the user whose birthday list is to be retrieved.</param>
        /// <returns>A list of users whose birthday is coming up in the next few days.</returns>
        [HttpGet(Routes.GetUsersWithComingBirthday)]
        public async Task<IActionResult> GetUsersWithComingBirthday(string userId)
        {
            try
            {
                var users = await _userService.GetAllUsersOtherThanAsync(userId);

                var today = DateTime.Today;

                DateTime startDate = today.AddDays(1);   // Tomorrow (excluding today)

                DateTime endDate = today.AddDays(Constants.BirthdayRange);     // 7 days forward

                var birthdayUsersInRange = users.Where(user => user.Birthday >= startDate && user.Birthday <= endDate);

                return Ok(birthdayUsersInRange);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }
    }
}
