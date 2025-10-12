using System.ComponentModel.DataAnnotations;

namespace DTOs.Request.Accounts
{
    public class SetRoleRequest
    {
        [Required(ErrorMessage = "User ID là bắt buộc")]
        public string UserId { get; set; } = string.Empty;

        [Required(ErrorMessage = "Role là bắt buộc")]
        [RegularExpression("^(Parent|Student|Teacher|Admin)$", ErrorMessage = "Role phải là Parent, Student, Teacher hoặc Admin")]
        public string Role { get; set; } = string.Empty;
    }
}
