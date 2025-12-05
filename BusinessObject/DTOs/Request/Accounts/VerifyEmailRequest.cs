using System.ComponentModel.DataAnnotations;

namespace BusinessObject.DTOs.Request.Accounts
{
    public class VerifyEmailRequest
    {
        [Required(ErrorMessage = "Email là bắt buộc")]
        [EmailAddress(ErrorMessage = "Email không hợp lệ")]
        public string Email { get; set; }

        [Required(ErrorMessage = "Mã xác thực là bắt buộc")]
        public string VerificationCode { get; set; }
    }
}
