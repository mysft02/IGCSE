using Swashbuckle.AspNetCore.Annotations;

namespace BusinessObject.DTOs.Request.Courses
{
    public class CourseFeedbackQueryRequest
    {
        [SwaggerSchema("Số trang (mặc định: 1)")]
        public int Page { get; set; } = 1;

        [SwaggerSchema("Số lượng item mỗi trang (mặc định: 10)")]
        public int PageSize { get; set; } = 10;

        [SwaggerSchema("Lọc theo rating (1-5, nếu có)")]
        public int? Rating { get; set; } = null;

        [SwaggerSchema("Tìm kiếm theo tên học viên (nếu có)")]
        public string? SearchByStudentName { get; set; } = null;

        [SwaggerSchema("Sắp xếp theo: 'date' (ngày tạo) hoặc 'rating' (điểm đánh giá). Mặc định: 'date'")]
        public string? SortBy { get; set; } = "date";

        [SwaggerSchema("Thứ tự sắp xếp: 'asc' (tăng dần) hoặc 'desc' (giảm dần). Mặc định: 'desc'")]
        public string? SortOrder { get; set; } = "desc";
    }
}


