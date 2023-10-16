using System.ComponentModel.DataAnnotations;

namespace HermesChat.Web.Models
{
    public class ForgotPasswordModel
    {
        [Required]
        [EmailAddress]
        public string? Email { get; set; }
    }
}
