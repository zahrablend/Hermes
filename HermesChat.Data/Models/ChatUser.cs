using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HermesChat.Data.Models
{
    public class ChatUser
    {
        public int Id { get; set; }
        public int ChatId { get; set; }
        public string? UserId { get; set; }
        public bool IsModerator { get; set; }
        public ChatUser(int id, int chatId, string? userId, bool isModerator)
        {
            Id = id;
            ChatId = chatId;
            UserId = userId;
            IsModerator = isModerator;
        }
    }
}
