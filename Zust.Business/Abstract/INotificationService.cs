using Zust.Entities.Models;

namespace Zust.Business.Abstract
{
    public interface INotificationService
    {
        /// <summary>
        /// Adds a new notification asynchronously.
        /// </summary>
        /// <param name="notification">The notification object to be added.</param>
        /// <returns>Task representing the asynchronous operation.</returns>
        Task AddAsync(Notification notification);

        /// <summary>
        /// Retrieves all notifications of a user asynchronously based on the user ID.
        /// </summary>
        /// <param name="userId">The ID of the user whose notifications will be retrieved.</param>
        /// <returns>A collection of Notification objects representing the user's notifications.</returns>
        Task<IEnumerable<Notification>> GetAllNotificationsOfUserAsync(string userId);

        /// <summary>
        /// Retrieves a notification by its ID asynchronously.
        /// </summary>
        /// <param name="notificationId">The ID of the notification to be retrieved.</param>
        /// <returns>The Notification object representing the notification.</returns>
        Task<Notification> GetNotificationByIdAsync(string notificationId);

        /// <summary>
        /// Retrieves the count of unread notifications for a user asynchronously based on the user ID.
        /// </summary>
        /// <param name="userId">The ID of the user.</param>
        /// <returns>The number of unread notifications.</returns>
        Task<int> GetUnreadNotificationCountAsync(string userId);

        /// <summary>
        /// Updates the "IsRead" status of a notification asynchronously.
        /// </summary>
        /// <param name="notificationid">The ID of the notification to be updated.</param>
        /// <returns>Task representing the asynchronous operation.</returns>
        Task UpdateNotificationIsReadAsync(string notificationid);

        /// <summary>
        /// Deletes all notifications of a user asynchronously based on the user ID.
        /// </summary>
        /// <param name="userId">The ID of the user whose notifications will be deleted.</param>
        /// <returns>Task representing the asynchronous operation.</returns>
        Task DeleteUserNotificationsAsync(string userId);

        /// <summary>
        /// Deletes a single notification asynchronously based on its ID.
        /// </summary>
        /// <param name="notificationId">The ID of the notification to delete.</param>
        /// <returns>Task representing the asynchronous operation.</returns>
        Task DeleteNotificationAsync(string notificationId);
    }
}
