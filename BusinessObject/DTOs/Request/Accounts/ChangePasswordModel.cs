using System.ComponentModel.DataAnnotations;

namespace BusinessObject.DTOs.Request.Accounts
{
    public class ChangePasswordModel
    {
        [Required]
        public string UserName { get; set; }

        [Required]
        public string CurrentPassword { get; set; }

        [Required]
        public string NewPassword { get; set; }

        [Required]
        public string ConfirmNewPassword { get; set; }
    }
}
