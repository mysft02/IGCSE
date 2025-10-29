using System.ComponentModel.DataAnnotations;

namespace BusinessObject.DTOs.Request.Accounts
{
    public class LoginRequest
    {
        [Required]
        public string Username { get; set; }
        [Required]
        public string Password { get; set; }
    }
}
