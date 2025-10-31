using Swashbuckle.AspNetCore.Annotations;

namespace BusinessObject.DTOs.Request.Accounts
{
    public class AccountListQuery
    {
        public int Page { get; set; } = 1;

        public int PageSize { get; set; } = 10;

        [SwaggerSchema("Tìm theo tên người dùng (nếu có)")]
        public string? SearchByName { get; set; } = null;

        [SwaggerSchema("Lọc theo role (nếu có): Admin/Teacher/Parent/Student")]
        public string? Role { get; set; } = null;

        [SwaggerSchema("Trạng thái hoạt động (nếu có)")]
        public bool? IsActive { get; set; } = null;
    }
}



