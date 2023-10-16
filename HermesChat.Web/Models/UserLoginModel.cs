using System.ComponentModel.DataAnnotations;
using System.Xml.Linq;

namespace HermesChat.Web.Models
{
    public class UserLoginModel
    {
        [Required]
        public string? Name { get; set; }
        [Required]
        [DataType(DataType.Password)]
        public string? Password { get; set; }
        [Display(Name = "Remember me?")]
        public bool RememberMe { get; set; }
    }
}
