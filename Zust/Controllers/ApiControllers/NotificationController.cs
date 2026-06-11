using Microsoft.AspNetCore.Mvc;
using Zust.Business.Abstract;
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
        /// Initializes a new instance of the NotificationController class with the specified services.
        /// </summary>
        /// <param name="notificationService">The service for handling notification-related operations.</param>
        /// <param name="userService">The service for handling user-related operations.</param>
        public NotificationController(INotificationService notificationService, IUserService userService)
        {
            _notificationService = notificationService;

            _userService = userService;
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
                
                // Sequential await on the shared DbContext (a `ForEach(async ...)` here ran these
                // as fire-and-forget tasks that raced the DbContext and could crash the process).
                foreach (var notification in notifications)
                {
                    notification.ToUser = await _userService.GetUserByIdAsync(notification.ToUserId);

                    notification.FromUser = await _userService.GetUserByIdAsync(notification.FromUserId);
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
