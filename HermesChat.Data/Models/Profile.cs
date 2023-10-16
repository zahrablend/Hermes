using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HermesChat.Data.Models
{
    public class Profile
    {
        public int Id { get; set; }
        public string? UserId { get; set; }
        public byte[]? Avatar { get; set; }
    }
}
