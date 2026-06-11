using Microsoft.AspNetCore.Mvc;
using Zust.Entities.Models;
using Zust.Web.Abstract;
using Zust.Web.Helpers.ConstantHelpers;

namespace Zust.Web.Controllers.ApiControllers
{
    /// <summary>
    /// Controller for managing static data and files in the application.
    /// </summary>
    [Route(Routes.Static)]
    [ApiController]
    public class StaticController : ControllerBase
    {
        /// <summary>
        /// The service responsible for handling static data and files in the application.
        /// </summary>
        private readonly IStaticService _staticService;

        /// <summary>
        /// Initializes a new instance of the StaticController class with the given IStaticService.
        /// </summary>
        /// <param name="staticService">The service responsible for handling static data and files.</param>
        public StaticController(IStaticService staticService)
        {
            _staticService = staticService;
        }

        /// <summary>
        /// Endpoint to get random status image paths.
        /// </summary>
        /// <returns>An ActionResult containing a list of random status image paths.</returns>
        [HttpGet(Routes.GetRandomStatusImagePaths)]
        public ActionResult<List<string>> GetRandomStatusImagePaths()
        {
            try
            {
                string path = Path.Combine(FileConstants.FilesFolderPath, FileConstants.StatusImagesFile);

                var paths = _staticService.GetRandomStatusImagePaths(Constants.StatusCountInNewsFeed, path);

                return Ok(paths);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        /// <summary>
        /// Endpoint to get watch videos.
        /// </summary>
        /// <returns>An ActionResult containing a list of watch videos.</returns>
        [HttpGet(Routes.GetWatchVideos)]
        public ActionResult<List<string>> GetWatchVideos()
        {
            try
            {
                string path = Path.Combine(FileConstants.FilesFolderPath, FileConstants.WatchVideoUrlsFile);

                var videos = _staticService.GetWatchVideos(path);

                return Ok(videos);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        /// <summary>
        /// Endpoint to get special users.
        /// </summary>
        /// <returns>An ActionResult containing a list of special users.</returns>
        [HttpGet(Routes.GetSpecialUsers)]
        public async Task<ActionResult<List<User>>> GetSpecialUsers()
        {
            try
            {
                var specialUsers = await _staticService.GetSpecialUsersAsync();

                // Project to only the fields the sidebar renders. Returning the full Identity User
                // would leak sensitive columns (PasswordHash, SecurityStamp, ...).
                var result = specialUsers.Select(u => new
                {
                    id = u.Id,
                    userName = u.UserName,
                    imageUrl = u.ImageUrl
                });

                return Ok(result);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }
    }
}
