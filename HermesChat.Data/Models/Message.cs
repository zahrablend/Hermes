using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HermesChat.Data.Models
{
    public class Message
    {
        public int Id { get; set; }
        public int ChatId { get; set; }
        public string? UserId { get; set; }
        public string? Content { get; set; }
        public DateTime Timestamp { get; set; }
        public bool IsEdited { get; set; }
        public Message(int id, int chatId, string? userId, string content, DateTime timestamp, bool isEdited)
        {
            Id = id;
            ChatId = chatId;
            UserId = userId;
            Content = content;
            Timestamp = timestamp;
            IsEdited = isEdited;
        }
    }
}
