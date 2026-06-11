using System.Collections.Generic;
using System.Threading.Tasks;
using Zust.Entities.Models;

namespace Zust.Business.Abstract
{
    public interface IUserService
    {
        /// <summary>
        /// Retrieves a user by their username asynchronously.
        /// </summary>
        /// <param name="username">The username of the user to be retrieved.</param>
        /// <returns>The User object representing the user with the given username. Returns null if not found.</returns>
        Task<User?> GetUserByUsernameAsync(string username);

        /// <summary>
        /// Checks if a username is already taken asynchronously.
        /// </summary>
        /// <param name="username">The username to be checked.</param>
        /// <returns>True if the username is already taken, otherwise false.</returns>
        Task<bool> UsernameIsTakenAsync(string username);

        /// <summary>
        /// Adds a new user asynchronously.
        /// </summary>
        /// <param name="user">The User object representing the new user to be added.</param>
        /// <returns>Task representing the asynchronous operation.</returns>
        Task AddAsync(User user);

        /// <summary>
        /// Retrieves a user by their ID asynchronously.
        /// </summary>
        /// <param name="id">The ID of the user to be retrieved.</param>
        /// <returns>The User object representing the user with the given ID. Returns null if not found.</returns>
        Task<User?> GetUserByIdAsync(string id);

        /// <summary>
        /// Updates an existing user asynchronously.
        /// </summary>
        /// <param name="user">The User object representing the user to be updated.</param>
        /// <returns>Task representing the asynchronous operation.</returns>
        Task UpdateAsync(User user);

        /// <summary>
        /// Retrieves all users asynchronously.
        /// </summary>
        /// <returns>A collection of User objects representing all users.</returns>
        Task<IEnumerable<User>> GetAllUsersAsync();

        /// <summary>
        /// Retrieves all users except the user with the specified ID asynchronously.
        /// </summary>
        /// <param name="userId">The ID of the user to be excluded from the result.</param>
        /// <returns>A collection of User objects representing all users except the specified user.</returns>
        Task<IEnumerable<User>> GetAllUsersOtherThanAsync(string userId);

        /// <summary>
        /// Retrieves a single page of users excluding the specified user, paginated at the database.
        /// </summary>
        /// <param name="userId">The ID of the user to be excluded from the result.</param>
        /// <param name="skip">Number of users to skip.</param>
        /// <param name="take">Maximum number of users to return.</param>
        Task<IEnumerable<User>> GetUsersOtherThanAsync(string userId, int skip, int take);

        /// <summary>
        /// Deletes a user by their ID asynchronously.
        /// </summary>
        /// <param name="userId">The ID of the user to be deleted.</param>
        /// <returns>Task representing the asynchronous operation.</returns>
        Task DeleteUserByIdAsync(string userId);
    }
}