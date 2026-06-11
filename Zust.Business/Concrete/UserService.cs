using Zust.Business.Abstract;
using Zust.DataAccess.Abstract;
using Zust.Entities.Models;

namespace Zust.Business.Concrete
{
    /// <summary>
    /// Represents a service that handles users.
    /// </summary>
    public class UserService : IUserService
    {
        /// <summary>
        /// Private field representing the data access layer for managing users.
        /// </summary>
        private readonly IUserDal _userDal;

        /// <summary>
        /// Initializes a new instance of the UserService class with the specified UserDal.
        /// </summary>
        /// <param name="userDal">The data access layer for handling users.</param>
        public UserService(IUserDal userDal)
        {
            _userDal = userDal;
        }

        /// <summary>
        /// Adds a new user asynchronously.
        /// </summary>
        /// <param name="user">The User object representing the user to be added.</param>
        /// <returns>Task representing the asynchronous operation.</returns>
        public async Task AddAsync(User user)
        {
            await _userDal.AddAsync(user);
        }

        /// <summary>
        /// Retrieves a user by their username asynchronously.
        /// </summary>
        /// <param name="username">The username of the user to retrieve.</param>
        /// <returns>The User object representing the user with the specified username.</returns>
        public async Task<User?> GetUserByUsernameAsync(string username)
        {
            return await _userDal.GetAsync(u => u.UserName.ToLower() == username.ToLower());
        }

        /// <summary>
        /// Checks if a username is already taken asynchronously.
        /// </summary>
        /// <param name="username">The username to check for availability.</param>
        /// <returns>True if the username is taken, otherwise false.</returns>
        public async Task<bool> UsernameIsTakenAsync(string username)
        {
            var user = await GetUserByUsernameAsync(username);

            return user != null;
        }

        /// <summary>
        /// Retrieves a user by their ID asynchronously.
        /// </summary>
        /// <param name="id">The ID of the user to retrieve.</param>
        /// <returns>The User object representing the user with the specified ID.</returns>
        public async Task<User?> GetUserByIdAsync(string id)
        {
            var user = await _userDal.GetAsync(u => u.Id == id);

            return user;
        }

        /// <summary>
        /// Updates an existing user asynchronously.
        /// </summary>
        /// <param name="user">The User object representing the user to be updated.</param>
        /// <returns>Task representing the asynchronous operation.</returns>
        public async Task UpdateAsync(User user)
        {
            await _userDal.UpdateAsync(user);
        }

        /// <summary>
        /// Retrieves all users asynchronously.
        /// </summary>
        /// <returns>A collection of User objects representing all users.</returns>
        public async Task<IEnumerable<User>> GetAllUsersAsync()
        {
            return await _userDal.GetAllAsync();
        }

        /// <summary>
        /// Retrieves all users except the specified user asynchronously.
        /// </summary>
        /// <param name="userId">The ID of the user to be excluded from the result.</param>
        /// <returns>A collection of User objects representing all users other than the specified user.</returns>
        public async Task<IEnumerable<User>> GetAllUsersOtherThanAsync(string userId)
        {
            var users = await GetAllUsersAsync();

            var list = users.ToList();

            list.RemoveAll(u => u.Id == userId);

            return list;
        }

        /// <summary>
        /// Retrieves a single page of users excluding the specified user, paginated at the database.
        /// </summary>
        /// <param name="userId">The ID of the user to be excluded from the result.</param>
        /// <param name="skip">Number of users to skip.</param>
        /// <param name="take">Maximum number of users to return.</param>
        public async Task<IEnumerable<User>> GetUsersOtherThanAsync(string userId, int skip, int take)
        {
            return await _userDal.GetUsersOtherThanAsync(userId, skip, take);
        }

        /// <summary>
        /// Deletes a user by their ID asynchronously.
        /// </summary>
        /// <param name="userId">The ID of the user to be deleted.</param>
        /// <returns>Task representing the asynchronous operation.</returns>
        public async Task DeleteUserByIdAsync(string userId)
        {
            var user = await _userDal?.GetAsync(u => u.Id == userId);

            if (user != null)
            {
                await _userDal.DeleteAsync(user);
            }
        }
    }
}
