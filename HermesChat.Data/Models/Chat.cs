using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HermesChat.Data.Models
{
    public class Chat
    {
        public int Id { get; set; }
        public int ChatUserId { get; set; }
        [EnumDataType(typeof(ChatType))]
        public ChatType ChatType { get; set; }
        public string? Name { get; set; }
        public Chat(int id, int chatUserId, string? name)
        {
            Id = id;
            ChatUserId = chatUserId;
            Name = name;
        }
    }
}
