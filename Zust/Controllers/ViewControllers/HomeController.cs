using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Zust.Business.Abstract;
using Zust.Entities.Models;
using Zust.Web.Abstract;
using Zust.Web.Helpers.ConstantHelpers;
using Zust.Web.Helpers.Utilities;
using Zust.Web.Models;

namespace Zust.Web.Controllers.ViewControllers
{
    /// <summary>
    /// Controller for managing home-related views and actions.
    /// </summary>
    [Authorize]
    [Controller]
    public class HomeController : Controller
    {
        /// <summary>
        /// Represents an instance of the IStaticService interface used to interact with static data and settings in the application.
        /// </summary>
        private readonly IStaticService _staticService;
        /// <summary>
        /// Represents an instance of the IUserService interface used to interact with user-related data and perform user-related operations in the application.
        /// </summary>
        private readonly IUserService _userService;

        /// <summary>
        /// Represents an instance of the IPostService interface used to interact with post-related data and perform post-related operations in the application.
        /// </summary>
        private readonly IPostService _postService;

        /// <summary>
        /// Represents an instance of the IChatService interface used to interact with chat-related data and perform chat-related operations in the application.
        /// </summary>
        private readonly IChatService _chatService;

        /// <summary>
        /// Represents an instance of the IMessageService interface used to interact with message-related data and perform message-related operations in the application.
        /// </summary>
        private readonly IMessageService _messageService;

        /// <summary>
        /// Initializes a new instance of the HomeController class with the specified services.
        /// </summary>
        /// <param name="userService">The user service to be used by the controller.</param>
        /// <param name="staticService">The static service to be used by the controller.</param>
        /// <param name="postService">The post service to be used by the controller.</param>
        /// <param name="chatService">The chat service to be used by the controller.</param>
        /// <param name="messageService">The message service to be used by the controller.</param>
        public HomeController(IUserService userService, IStaticService staticService, IPostService postService, IChatService chatService, IMessageService messageService)
        {
            _userService = userService;

            _staticService = staticService;

            _postService = postService;

            _chatService = chatService;

            _messageService = messageService;
        }

        /// <summary>
        /// Displays the Birthday view.
        /// </summary>
        /// <returns>The Birthday view.</returns>
        public IActionResult Birthday()
        {
            return View();
        }

        /// <summary>
        /// Displays the user profile page for a specific user.
        /// If the "id" parameter is empty, returns the default view.
        /// If the user with the given "id" is not found, returns a 404 Not Found page.
        /// </summary>
        /// <param name="id">The unique identifier of the user.</param>
        /// <returns>Returns a view containing the user profile information if the user is found, 
        /// otherwise returns a default view or a 404 Not Found page.</returns>
        public async Task<IActionResult> Users(string id = Constants.StringEmpty)
        {
            if (id == Constants.StringEmpty)
            {
                return View();
            }

            var user = await _userService.GetUserByIdAsync(id);

            if (user == null)
            {
                return NotFound();
            }

            return View(Routes.UserProfile, user);
        }

        /// <summary>
        /// Displays the details of a specific post with the given "id".
        /// If the "id" parameter is empty, returns a 404 Not Found page.
        /// If the post with the given "id" is not found, returns a 404 Not Found page.
        /// </summary>
        /// <param name="id">The unique identifier of the post.</param>
        /// <returns>Returns a view containing the post details if the post is found, 
        /// otherwise returns a 404 Not Found page.</returns>
        public async Task<IActionResult> Posts(string id = Constants.StringEmpty)
        {
            if (id == Constants.StringEmpty)
            {
                return NotFound();
            }

            var post = await _postService.GetPostByIdAsync(id);

            if (post == null)
            {
                return NotFound();
            }

            post.User = await _userService.GetUserByIdAsync(post.UserId);

            return View(Routes.Post, post);
        }


        /// <summary>
        /// Displays the chat view for a specific user with the given "userId".
        /// If the "userId" parameter is empty or null, returns a 404 Not Found page.
        /// If the user with the given "userId" is not found, returns a 404 Not Found page.
        /// </summary>
        /// <param name="userId">The unique identifier of the user to chat with.</param>
        /// <returns>Returns a view containing the chat details if the user is found and 
        /// a chat exists, otherwise returns a 404 Not Found page.</returns>
        public async Task<IActionResult> Chats(string userId = Constants.StringEmpty)
        {
            if (userId == String.Empty)
            {
                return View();
            }

            var currentUser = await UserHelper.GetCurrentUserAsync(HttpContext);

            var userToChat = await _userService.GetUserByIdAsync(userId);

            if (userToChat == null)
            {
                return NotFound();
            }

            var chat = await _chatService.GetChatAsync(currentUser.Id, userToChat.Id);

            if (chat == null)
            {
                chat = new Chat()
                {
                    Id = Guid.NewGuid().ToString(),

                    SenderUserId = currentUser.Id,

                    ReceiverUserId = userToChat.Id,

                    SenderUser = currentUser,

                    ReceiverUser = userToChat
                };

                await _chatService.AddChatAsync(chat);
            }


            var messages = await _messageService.GetChatMessagesByIdAsync(chat.Id);

            if (messages != null)
            {
                // Sequential: shared scoped DbContext does not allow concurrent operations.
                foreach (var m in messages)
                {
                    m.SenderUser = await _userService.GetUserByIdAsync(m.SenderUserId);

                    m.ReceiverUser = await _userService.GetUserByIdAsync(m.ReceiverUserId);
                }

                messages = messages.OrderBy(m => m.DateSent).ToList();

                chat.Messages = messages.ToList();
            }
            else
            {
                chat.Messages = new List<Message>();
            }

            var chatForOtherUser = await _chatService.GetChatAsync(userToChat.Id, currentUser.Id);

            if (chatForOtherUser == null)
            {
                chatForOtherUser = new Chat()
                {
                    Id = Guid.NewGuid().ToString(),

                    SenderUserId = userToChat.Id,

                    ReceiverUserId = currentUser.Id,

                    SenderUser = userToChat,

                    ReceiverUser = currentUser
                };

                await _chatService.AddChatAsync(chatForOtherUser);
            }

            var chatVm = new ChatViewModel()
            {
                CurrentUser = currentUser,

                UserToChat = userToChat,

                Chat = chat,

                ChatForOtherUser = chatForOtherUser
            };

            return View(Routes.Chat, chatVm);
        }


        /// <summary>
        /// Displays the Events view.
        /// </summary>
        /// <returns>The Events view.</returns>
        public IActionResult Events()
        {
            return View();
        }

        /// <summary>
        /// Displays the Favorite view.
        /// </summary>
        /// <returns>The Favorite view.</returns>
        public IActionResult Favorite()
        {
            return View();
        }

        /// <summary>
        /// Displays the Friends view.
        /// </summary>
        /// <returns>The Friends view.</returns>
        public IActionResult Friends()
        {
            return View();
        }

        /// <summary>
        /// Displays the Groups view.
        /// </summary>
        /// <returns>The Groups view.</returns>
        public IActionResult Groups()
        {
            return View();
        }

        /// <summary>
        /// Displays the Help and Support view.
        /// </summary>
        /// <returns>The Help and Support view.</returns>
        [HttpGet($"{Routes.Home}/{Routes.HelpAndSupport}")]
        public IActionResult HelpAndSupport()
        {
            return View(Routes.HelpAndSupport);
        }

        /// <summary>
        /// Displays the Help and Support view.
        /// </summary>
        /// <returns>The Help and Support view.</returns>
        [HttpGet($"{Routes.Home}/{Routes.FriendRequests}")]
        public IActionResult FriendRequests()
        {
            return View(Routes.FriendRequests);
        }

        /// <summary>
        /// Displays the Index view.
        /// </summary>
        /// <returns>The Index view.</returns>
        public IActionResult Index()
        {
            var vm = new IndexViewModel()
            {
                CreatePostViewModel = new CreatePostViewModel(),
                WatchVideos = _staticService.GetWatchVideos(Path.Combine(FileConstants.FilesFolderPath, FileConstants.WatchVideoUrlsFile)),
                Advertisements = _staticService.GetAdvertisements(Path.Combine(FileConstants.FilesFolderPath, FileConstants.AdvertisementsFile))
            };
            return View(vm);
        }

        /// <summary>
        /// Displays the Live Chat view.
        /// </summary>
        /// <returns>The Live Chat view.</returns>
        [HttpGet($"{Routes.Home}/{Routes.LiveChat}")]
        public IActionResult LiveChat()
        {
            return View(Routes.LiveChat);
        }

        /// <summary>
        /// Displays the Marketplace view.
        /// </summary>
        /// <returns>The Marketplace view.</returns>
        public IActionResult Marketplace()
        {
            return View();
        }

        /// <summary>
        /// Displays the MyProfile view.
        /// </summary>
        /// <returns>The MyProfile view.</returns>
        [HttpGet($"{Routes.Home}/{Routes.MyProfile}")]
        public IActionResult MyProfile()
        {
            return View(Routes.MyProfile);
        }

        /// <summary>
        /// Displays the Notifications view.
        /// </summary>
        /// <returns>The Notifications view.</returns>
        public IActionResult Notifications()
        {
            return View();
        }

        /// <summary>
        /// Displays the Privacy view.
        /// </summary>
        /// <returns>The Privacy view.</returns>
        public IActionResult Privacy()
        {
            return View();
        }

        /// <summary>
        /// Displays the Video view.
        /// </summary>
        /// <returns>The Video view.</returns>
        public IActionResult Video()
        {
            return View();
        }

        /// <summary>
        /// Displays the Weather view.
        /// </summary>
        /// <returns>The Weather view.</returns>
        public IActionResult Weather()
        {
            return View();
        }

        /// <summary>
        /// Displays the Error view.
        /// </summary>
        /// <returns>The Error view.</returns>
        [AllowAnonymous]
        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View();
        }
    }
}