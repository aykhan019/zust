namespace Zust.Web.Models
{
    /// <summary>
    /// Represents a single entry in the chat list: the chat partner together with the
    /// text of the last message exchanged. Returned by the GetChatList endpoint so the
    /// chats page can render every conversation in a single response instead of issuing
    /// one last-message request per chat.
    /// </summary>
    public class ChatListItemViewModel
    {
        /// <summary>
        /// Gets or sets the ID of the chat partner.
        /// </summary>
        public string Id { get; set; }

        /// <summary>
        /// Gets or sets the user name of the chat partner.
        /// </summary>
        public string UserName { get; set; }

        /// <summary>
        /// Gets or sets the profile image URL of the chat partner.
        /// </summary>
        public string ImageUrl { get; set; }

        /// <summary>
        /// Gets or sets the text of the last message exchanged with the chat partner.
        /// </summary>
        public string LastMessage { get; set; }

        /// <summary>
        /// Gets or sets the time the last message was sent. Used to order the chat list so the
        /// most recent conversation appears on top.
        /// </summary>
        public DateTime LastMessageDate { get; set; }
    }
}
