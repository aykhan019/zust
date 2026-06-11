using Microsoft.AspNetCore.Mvc;
using Zust.Business.Abstract;
using Zust.DataAccess.Abstract;
using Zust.Entities.Models;
using Zust.Web.Helpers.ConstantHelpers;

namespace Zust.Web.Controllers.ApiControllers
{
    /// <summary>
    /// API controller responsible for handling notifications and related operations.
    /// </summary>
    [Route(Routes.NotificationAPI)]
    [ApiController]
    public class NotificationController : ControllerBase
    {
        /// <summary>
        /// The service responsible for handling notification-related operations.
        /// </summary>
        private readonly INotificationService _notificationService;

        /// <summary>
        /// The service responsible for handling user-related operations.
        /// </summary>
        private readonly IUserService _userService;

        /// <summary>
        /// The data access layer for fetching only the users referenced by a set of notifications.
        /// </summary>
        private readonly IUserDal _userDal;

        /// <summary>
        /// Initializes a new instance of the NotificationController class with the specified services.
        /// </summary>
        /// <param name="notificationService">The service for handling notification-related operations.</param>
        /// <param name="userService">The service for handling user-related operations.</param>
        /// <param name="userDal">The data access layer for fetching users by id.</param>
        public NotificationController(INotificationService notificationService, IUserService userService, IUserDal userDal)
        {
            _notificationService = notificationService;

            _userService = userService;

            _userDal = userDal;
        }

        /// <summary>
        /// Gets notifications of a user by their ID.
        /// </summary>
        /// <param name="userId">The ID of the user whose notifications to retrieve.</param>    
        [HttpGet(Routes.GetNotificationsOfUser)]
        public async Task<ActionResult<IEnumerable<Notification>>> GetNotificationsOfUser(string userId)
        {
            try
            {
                var notifications = (await _notificationService.GetAllNotificationsOfUserAsync(userId)).ToList();

                // Load only the users referenced by these notifications, in one WHERE ... IN query,
                // then assign from an in-memory lookup. (Loading the entire users table to resolve
                // a handful of referenced users was needlessly slow.)
                var referencedUserIds = notifications
                    .SelectMany(n => new[] { n.ToUserId, n.FromUserId })
                    .Where(id => id is not null)
                    .Distinct()
                    .ToHashSet();

                var usersById = referencedUserIds.Count == 0
                    ? new Dictionary<string, User>()
                    : (await _userDal.GetAllAsync(u => referencedUserIds.Contains(u.Id)))
                          .GroupBy(u => u.Id)
                          .ToDictionary(g => g.Key, g => g.First());

                foreach (var notification in notifications)
                {
                    usersById.TryGetValue(notification.ToUserId, out var toUser);
                    usersById.TryGetValue(notification.FromUserId, out var fromUser);
                    notification.ToUser = toUser;
                    notification.FromUser = fromUser;
                }

                return Ok(notifications);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        /// <summary>
        /// Marks a notification as read.
        /// </summary>
        /// <param name="notificationId">The ID of the notification to mark as read.</param>
        [HttpPost(Routes.SetNotificationRead)]
        public async Task<ActionResult<IEnumerable<Notification>>> SetNotificationRead(string notificationId)
        {
            try
            {
                var notification = await _notificationService.GetNotificationByIdAsync(notificationId);

                await _notificationService.UpdateNotificationIsReadAsync(notificationId);

                return Ok();
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        /// <summary>
        /// Gets the count of unread notifications for a user by their ID.
        /// </summary>
        /// <param name="userId">The ID of the user whose unread notification count to retrieve.</param>
        [HttpGet(Routes.GetUnreadNotificationCount)]
        public async Task<ActionResult<IEnumerable<Notification>>> GetUnseenNotificationCount(string userId)
        {
            try
            {
                var count = await _notificationService.GetUnreadNotificationCountAsync(userId);

                return Ok(count);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }
    }
}
