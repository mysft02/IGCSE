using Swashbuckle.AspNetCore.Annotations;

namespace BusinessObject.DTOs.Request.Notifications
{
    public class NotificationQueryRequest
    {
        [SwaggerSchema("Số trang (mặc định: 1)")]
        public int Page { get; set; } = 1;

        [SwaggerSchema("Số lượng item mỗi trang (mặc định: 10)")]
        public int PageSize { get; set; } = 10;

        [SwaggerSchema("Lọc theo trạng thái đọc: true (đã đọc), false (chưa đọc), null (tất cả)")]
        public bool? IsRead { get; set; }

        [SwaggerSchema("Lọc theo loại notification: COURSE_DISABLED, COURSE_APPROVED, COURSE_REJECTED, ...")]
        public string? Type { get; set; }

        [SwaggerSchema("Lọc theo loại entity liên quan: COURSE, REPORT, ...")]
        public string? RelatedEntityType { get; set; }

        [SwaggerSchema("Tìm kiếm theo tiêu đề notification")]
        public string? SearchByTitle { get; set; }
    }
}
