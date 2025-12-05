using System.ComponentModel.DataAnnotations;

namespace BusinessObject.DTOs.Request.Accounts
{
    public class RegisterRequest
    {
        [Required(ErrorMessage = "Username là bắt buộc")]
        [StringLength(10, ErrorMessage = "Username tối đa 10 ký tự")]
        [RegularExpression("^[a-zA-Z0-9]+$", ErrorMessage = "Username chỉ được chứa chữ và số")]
        public string Username { get; set; }

        [Required(ErrorMessage = "Tên là bắt buộc")]
        [StringLength(20, ErrorMessage = "Tên tối đa 20 ký tự")]
        [RegularExpression(@"^[\p{L}\s]+$", ErrorMessage = "Tên chỉ được chứa chữ cái và khoảng trắng")]
        public string FullName { get; set; }

        [Required(ErrorMessage = "Email là bắt buộc")]
        [EmailAddress(ErrorMessage = "Email không hợp lệ, vui lòng thêm @ vào địa chỉ email")]
        [StringLength(20, ErrorMessage = "Email tối đa 20 ký tự")]
        public string Email { get; set; }

        [Required(ErrorMessage = "Địa chỉ là bắt buộc")]
        [StringLength(50, ErrorMessage = "Địa chỉ tối đa 50 ký tự")]
        public string Address { get; set; }

        [Required(ErrorMessage = "Số điện thoại là bắt buộc")]
        [RegularExpression(@"^0[0-9]{9,10}$", ErrorMessage = "Số điện thoại phải bắt đầu bằng 0 và có 10 chữ số")]
        public string Phone { get; set; }

        [Required(ErrorMessage = "Mật khẩu là bắt buộc")]
        [StringLength(20, MinimumLength = 8, ErrorMessage = "Mật khẩu phải từ 8 đến 20 ký tự")]
        [RegularExpression(@"^(?=.*[a-z])(?=.*[A-Z])(?=.*\d)(?=.*[\W_]).+$", ErrorMessage = "Mật khẩu phải có chữ hoa, chữ thường, số và ký tự đặc biệt")]
        public string? Password { get; set; }

        [Required(ErrorMessage = "Ngày sinh là bắt buộc")]
        public DateTime DateOfBirth { get; set; }
    }
}
