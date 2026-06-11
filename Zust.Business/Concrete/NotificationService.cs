using Zust.Business.Abstract;
using Zust.DataAccess.Abstract;
using Zust.Entities.Models;

namespace Zust.Business.Concrete
{
    /// <summary>
    /// Represents a service that handles notifications.
    /// </summary>
    public class NotificationService : INotificationService
    {
        /// <summary>
        /// Private field representing the data access layer for managing notifications.
        /// </summary>
        private readonly INotificationDal _notificationDal;

        /// <summary>
        /// Initializes a new instance of the NotificationService class with the specified NotificationDal.
        /// </summary>
        /// <param name="notificationDal">The data access layer for handling notifications.</param>
        public NotificationService(INotificationDal notificationDal)
        {
            _notificationDal = notificationDal;
        }

        /// <summary>
        /// Adds a new notification asynchronously.
        /// </summary>
        /// <param name="notification">The Notification object representing the notification to be added.</param>
        /// <returns>Task representing the asynchronous operation.</returns>
        public async Task AddAsync(Notification notification)
        {
            await _notificationDal.AddAsync(notification);
        }

        /// <summary>
        /// Deletes all notifications associated with a user asynchronously.
        /// </summary>
        /// <param name="userId">The ID of the user whose notifications will be deleted.</param>
        /// <returns>Task representing the asynchronous operation.</returns>
        public async Task DeleteUserNotificationsAsync(string userId)
        {
            var notifications = await _notificationDal.GetAllAsync(n => n.FromUserId == userId || n.ToUserId == userId);

            if (notifications != null)
            {
                foreach (var notification in notifications)
                {
                    await _notificationDal.DeleteAsync(notification);
                }
            }
        }

        /// <summary>
        /// Deletes a single notification asynchronously based on its ID.
        /// </summary>
        /// <param name="notificationId">The ID of the notification to delete.</param>
        /// <returns>Task representing the asynchronous operation.</returns>
        public async Task DeleteNotificationAsync(string notificationId)
        {
            var notification = await _notificationDal.GetAsync(n => n.Id == notificationId);

            if (notification != null)
            {
                await _notificationDal.DeleteAsync(notification);
            }
        }

        /// <summary>
        /// Retrieves all notifications sent to a specific user asynchronously.
        /// </summary>
        /// <param name="userId">The ID of the user to retrieve notifications for.</param>
        /// <returns>A collection of Notification objects representing the notifications sent to the user.</returns>
        public async Task<IEnumerable<Notification>> GetAllNotificationsOfUserAsync(string userId)
        {
            return await _notificationDal.GetAllAsync(n => n.ToUserId == userId);
        }

        /// <summary>
        /// Retrieves a notification by its ID asynchronously.
        /// </summary>
        /// <param name="notificationId">The ID of the notification to retrieve.</param>
        /// <returns>The Notification object representing the notification with the specified ID.</returns>
        public async Task<Notification?> GetNotificationByIdAsync(string notificationId)
        {
            return await _notificationDal.GetAsync(n => n.Id == notificationId);
        }

        /// <summary>
        /// Gets the count of unread notifications for a user asynchronously.
        /// </summary>
        /// <param name="userId">The ID of the user to get the unread notification count for.</param>
        /// <returns>The number of unread notifications for the user.</returns>
        public async Task<int> GetUnreadNotificationCountAsync(string userId)
        {
            var notifications = await GetAllNotificationsOfUserAsync(userId);

            return notifications.Count(n => !n.IsRead);
        }

        /// <summary>
        /// Updates the IsRead property of a notification to mark it as read asynchronously.
        /// </summary>
        /// <param name="notificationid">The ID of the notification to mark as read.</param>
        /// <returns>Task representing the asynchronous operation.</returns>
        public async Task UpdateNotificationIsReadAsync(string notificationid)
        {
            var notification = await GetNotificationByIdAsync(notificationid);

            if (notification != null)
            {
                notification.IsRead = true;

                await _notificationDal.UpdateAsync(notification);
            }
        }
    }
}