using Microsoft.AspNetCore.Mvc;
using Zust.Web.Helpers.ConstantHelpers;
using Zust.Web.Models;

namespace Zust.Web.Controllers.ViewControllers
{
    /// <summary>
    /// Controller for user account management.
    /// </summary>
    [Controller]
    public class AccountController : Controller
    {
        /// <summary>
        /// Displays the public landing page.
        /// </summary>
        /// <returns>The landing view.</returns>
        public IActionResult Landing()
        {
            return View();
        }

        /// <summary>
        /// Displays the login view.
        /// </summary>
        /// <returns>The login view.</returns>
        public IActionResult Login(LoginViewModel vm)
        {
            return View(vm);
        }

        /// <summary>
        /// Displays the register view with an empty register model.
        /// </summary>
        /// <returns>The register view.</returns>
        public IActionResult Register(RegisterViewModel vm)
        {
            return View(vm);
        }

        /// <summary>
        /// Displays the settings view.
        /// </summary>  
        /// <returns>The settings view.</returns>
        public IActionResult Settings()
        {
            return View();
        }

        /// <summary>
        /// Displays the forgot password view.
        /// </summary>
        /// <returns>The forgot password view.</returns>
        [HttpGet($"{Routes.Account}/{Routes.ForgotPassword}")]
        public IActionResult ForgotPassword()
        {
            return View(Routes.ForgotPassword);
        }
    }
}
