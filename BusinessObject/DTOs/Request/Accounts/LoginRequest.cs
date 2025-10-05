using System.ComponentModel.DataAnnotations;

namespace DTOs.Request.Accounts
{
    public class LoginRequest
    {
        [Required]
        public string Username { get; set; }
        [Required]
        public string Password { get; set; }
    }
}
