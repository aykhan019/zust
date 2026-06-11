using Microsoft.AspNetCore.Mvc;
using Zust.Business.Abstract;
using Zust.Entities.Models;
using Zust.Web.Helpers.ConstantHelpers;
using Zust.Web.Helpers.Utilities;
using Zust.Web.Models;

namespace Zust.Web.Controllers.ApiControllers
{
    /// <summary>
    /// API controller responsible for handling chat-related operations.
    /// </summary>
    [Route(Routes.ChatAPI)]
    [ApiController]
    public class ChatController : ControllerBase
    {
        /// <summary>
        /// Gets the user service used by the controller.
        /// </summary>
        private readonly IUserService _userService;

        /// <summary>
        /// Gets the chat service used by the controller.
        /// </summary>
        private readonly IChatService _chatService;

        /// <summary>
        /// Gets the message service used by the controller.
        /// </summary>
        private readonly IMessageService _messageService;

        /// <summary>
        /// Gets the notification service used by the controller.
        /// </summary>
        private readonly INotificationService _notificationService;

        /// <summary>
        /// Initializes a new instance of the <see cref="ChatController"/> class with the specified services.
        /// </summary>
        /// <param name="chatService">The chat service to be used by the controller.</param>
        /// <param name="messageService">The message service to be used by the controller.</param>
        /// <param name="userService">The user service to be used by the controller.</param>
        /// <param name="notificationService">The notification service to be used by the controller.</param>
        public ChatController(IChatService chatService, IMessageService messageService, IUserService userService, INotificationService notificationService)
        {
            _chatService = chatService;

            _messageService = messageService;

            _userService = userService;

            _notificationService = notificationService;
        }

        /// <summary>
        /// Adds a new message to the chat.
        /// </summary>
        /// <param name="model">The SendMessageViewModel containing the message data.</param>
        /// <returns>Returns ActionResult with a MessageNotificationViewModel on success, or BadRequest with an error message on failure.</returns>
        [HttpPost(Routes.AddMessage)]
        public async Task<ActionResult<Message>> AddMessage([FromBody] SendMessageViewModel model)
        {
            try
            {
                // For Current User
                var message = new Message()
                {
                    Id = Guid.NewGuid().ToString(),
                    Text = model.Message.Text,
                    ReceiverUserId = model.Message.ReceiverUserId,
                    SenderUserId = model.Message.SenderUserId,
                    ChatId = model.Message.ChatId,
                    DateSent = DateTime.Now
                };

                await _messageService.AddMessageAsync(message);

                message.ReceiverUser = await _userService.GetUserByIdAsync(model.Message.ReceiverUserId);

                message.SenderUser = await _userService.GetUserByIdAsync(model.Message.SenderUserId);

                message.Chat = await _chatService.GetChatByIdAsync(model.Message.ChatId);

                // For User To Send Message
                var otherUserChat = await _chatService.GetChatAsync(model.Message.ReceiverUserId, model.Message.SenderUserId);

                var message2 = new Message()
                {
                    Id = Guid.NewGuid().ToString(),

                    Text = model.Message.Text,

                    ReceiverUserId = model.Message.ReceiverUserId,

                    SenderUserId = model.Message.SenderUserId,

                    ChatId = otherUserChat.Id,

                    DateSent = DateTime.Now
                };

                await _messageService.AddMessageAsync(message2);

                var currentUser = await UserHelper.GetCurrentUserAsync(HttpContext);

                var messageNotificationVM = new MessageNotificationViewModel()
                {
                    Message = message,
                    Notification = null
                };

                if (!model.FirstMessageSent)
                {

                    var notification = new Notification()
                    {
                        Id = Guid.NewGuid().ToString(),

                        Date = DateTime.Now,

                        IsRead = false,

                        FromUserId = currentUser.Id,

                        FromUser = currentUser,

                        ToUserId = model.Message.ReceiverUserId,

                        ToUser = await _userService.GetUserByIdAsync(model.Message.ReceiverUserId),

                        Message = NotificationType.GetSentYouMessageMessage(currentUser.UserName),
                    };

                    messageNotificationVM.Notification = notification;

                    await _notificationService.AddAsync(notification);
                }

                // Success
                return Ok(messageNotificationVM);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        /// <summary>
        /// Gets all chat users for a specific user.
        /// </summary>
        /// <param name="userId">The ID of the user whose chats need to be retrieved.</param>
        /// <returns>Returns ActionResult with a list of users representing the chat participants on success, or BadRequest with an error message on failure.</returns>
        [HttpGet(Routes.GetChats)]
        public async Task<ActionResult<IEnumerable<User>>> GetChats(string userId)
        {
            try
            {
                var chats = await _chatService.GetAllUserChats(userId);

                var list = chats.ToList();

                var currentUser = await UserHelper.GetCurrentUserAsync(HttpContext);

                // Sequential: shared scoped DbContext does not allow concurrent operations
                // (the previous Task.WhenAll raced the context and made this endpoint fail).
                var users = new List<User?>(list.Count);
                foreach (var c in list)
                {
                    if (c.SenderUserId != currentUser.Id)
                    {
                        users.Add(await _userService.GetUserByIdAsync(c.SenderUserId));
                    }
                    else if (c.ReceiverUserId != currentUser.Id)
                    {
                        users.Add(await _userService.GetUserByIdAsync(c.ReceiverUserId));
                    }
                    else
                    {
                        users.Add(null); // neither sender nor receiver matched the current user
                    }
                }

                return Ok(users);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        /// <summary>
        /// Gets the full chat list for a user: every chat partner together with the text of
        /// the last message exchanged, in a single response. This lets the chats page render
        /// all conversations at once instead of issuing one GetLastMessage request per chat
        /// (which made the chats appear one by one).
        /// </summary>
        /// <param name="userId">The ID of the user whose chat list is requested.</param>
        /// <returns>A list of chat list items, ordered by most recent message first.</returns>
        [HttpGet(Routes.GetChatList)]
        public async Task<ActionResult<IEnumerable<ChatListItemViewModel>>> GetChatList(string userId)
        {
            try
            {
                var chats = (await _chatService.GetAllUserChats(userId)).ToList();

                var currentUser = await UserHelper.GetCurrentUserAsync(HttpContext);

                // Build each entry sequentially: the scoped DbContext is shared and does not
                // support concurrent operations.
                var items = new List<ChatListItemViewModel>(chats.Count);
                foreach (var chat in chats)
                {
                    var partnerId = chat.SenderUserId != currentUser.Id
                        ? chat.SenderUserId
                        : chat.ReceiverUserId;

                    if (partnerId == currentUser.Id)
                    {
                        continue; // neither side matched another user
                    }

                    var partner = await _userService.GetUserByIdAsync(partnerId);

                    if (partner == null)
                    {
                        continue; // chat partner no longer has an account
                    }

                    var lastMessage = await _messageService.GetLastMessageAsync(chat);

                    if (lastMessage == null || string.IsNullOrEmpty(lastMessage.Text))
                    {
                        continue; // no conversation yet, mirror the previous client behaviour
                    }

                    items.Add(new ChatListItemViewModel
                    {
                        Id = partner.Id,
                        UserName = partner.UserName,
                        ImageUrl = partner.ImageUrl,
                        LastMessage = lastMessage.Text,
                        LastMessageDate = lastMessage.DateSent
                    });
                }

                // Most recent conversation first, so the latest chat sits on top of the list.
                var ordered = items.OrderByDescending(i => i.LastMessageDate);

                return Ok(ordered);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        /// <summary>
        /// Retrieves the last message between the current user and the specified user.
        /// </summary>
        /// <param name="userId">The ID of the user with whom to retrieve the last message.</param>
        /// <returns>
        /// An asynchronous operation that returns an <see cref="ActionResult"/> containing the text of the last message,
        /// or an empty string if there is no last message, or a <see cref="BadRequestResult"/> if an error occurs during the process.
        /// </returns>
        [HttpGet(Routes.GetLastMessage)]
        public async Task<ActionResult<string>> GetLastMessage(string userId)
        {
            try
            {
                var currentUser = await UserHelper.GetCurrentUserAsync(HttpContext);

                var chat = await _chatService.GetChatAsync(currentUser.Id, userId);

                var message = await _messageService.GetLastMessageAsync(chat);

                if (message == null)
                {
                    return Ok(String.Empty);
                }
                else
                {
                    return Ok(message.Text);
                }
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }
    }
}
