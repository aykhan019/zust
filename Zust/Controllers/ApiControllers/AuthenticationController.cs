using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Zust.Business.Abstract;
using Zust.Entities.Models;
using Zust.Web.Helpers.ConstantHelpers;
using Zust.Web.Models;

namespace Zust.Web.Controllers.ApiControllers
{
    /// <summary>
    /// Controller responsible for authentication-related actions, such as user registration and login.
    /// </summary>
    [Route(Routes.Authentication)]
    [Controller]
    public class AuthenticationController : ControllerBase
    {
        /// <summary>
        /// Gets the manager for user sign-in functionality.
        /// </summary>
        private readonly SignInManager<User> _signInManager;

        /// <summary>
        /// The user manager component used for managing user-related operations.
        /// </summary>
        private readonly UserManager<User> _userManager;

        /// <summary>
        /// The role manager component used for managing roles.
        /// </summary>
        private readonly RoleManager<Zust.Entities.Models.Role> _roleManager;

        /// <summary>
        /// The IUserService component used for user-related operations.
        /// </summary>
        private readonly IUserService _userService;

        /// <summary>
        /// Initializes a new instance of the AuthenticationController class with the specified services and managers.
        /// </summary>
        /// <param name="signInManager">The manager for handling sign-in operations for users.</param>
        /// <param name="userManager">The manager for handling user-related operations.</param>
        /// <param name="roleManager">The manager for handling role-related operations.</param>
        /// <param name="userService">The service for handling user-related operations.</param>
        public AuthenticationController(SignInManager<User> signInManager,
                                        UserManager<User> userManager,
                                        RoleManager<Zust.Entities.Models.Role> roleManager,
                                        IUserService userService)
        {
            _signInManager = signInManager;

            _userManager = userManager;

            _roleManager = roleManager; 

            _userService = userService;
        }

        /// <summary>
        /// Handles the registration process for a new user.
        /// </summary>
        /// <param name="model">The RegisterViewModel containing the user's registration details.</param>
        /// <returns>The appropriate ActionResult based on the registration result.</returns>
        [HttpPost(Routes.Register)]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Register(RegisterViewModel model)
        {
            if (await _userService.UsernameIsTakenAsync(model.Username))
            {
                model.Errors.Add(Errors.UsernameIsTakenError);

                return RedirectToAction(Routes.Register, Routes.Account, routeValues: model);
            }

            // Create a new User object and populate its properties
            var user = new User
            {
                UserName = model.Username.Trim(),

                Email = model.Email.Trim(),

                CoverImage = Constants.DefaultCoverImagePath,

                ImageUrl = Constants.GetRandomProfileImagePath()
            };

            // Register the user with the provided password
            var result = await _userManager.CreateAsync(user, model.Password);

            if (result.Succeeded)
            {
                // Assign the "User" role to the registered user
                if (!_roleManager.RoleExistsAsync(Helpers.ConstantHelpers.Role.User).Result)
                {
                    var role = new Zust.Entities.Models.Role
                    {
                        Name = Helpers.ConstantHelpers.Role.User
                    };

                    await _roleManager.CreateAsync(role);
                }

                _userManager.AddToRoleAsync(user, Helpers.ConstantHelpers.Role.User).Wait();

                // Sign the user in
                await _signInManager.SignInAsync(user, isPersistent: false);

                return RedirectToAction(Routes.Index, Routes.Home);
            }
            else
            {
                result.Errors.ToList().ForEach(error => { model.Errors.Add(error.Description); });

                return RedirectToAction(Routes.Register, Routes.Account, routeValues: model);
            }
        }

        /// <summary>
        /// Handles the login process for a user.
        /// </summary>
        /// <param name="model">The LoginViewModel containing the user's login credentials.</param>
        /// <returns>The appropriate ActionResult based on the login result.</returns>
        [HttpPost(Routes.Login)]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Login(LoginViewModel model)
        {
            if (ModelState.IsValid)
            {
                var result = await _signInManager.PasswordSignInAsync(model.Username.Trim(), model.Password.Trim(), model.RememberMe, lockoutOnFailure: false);

                if (result.Succeeded)
                {
                    return RedirectToAction(Routes.Index, Routes.Home);
                }
            }

            model.Errors.Add(Errors.InvalidLoginError);

            return RedirectToAction(Routes.Login, Routes.Account, routeValues: model);
        }
    }
}
