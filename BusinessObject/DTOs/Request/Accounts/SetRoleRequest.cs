using Common.Constants;
using System.ComponentModel.DataAnnotations;

namespace BusinessObject.DTOs.Request.Accounts
{
    public class SetRoleRequest
    {
        [Required(ErrorMessage = "User ID là bắt buộc")]
        public string UserId { get; set; } = string.Empty;

        [Required(ErrorMessage = "Role là bắt buộc")]
        public UserRoleEnum Role { get; set; } 
    }
}
