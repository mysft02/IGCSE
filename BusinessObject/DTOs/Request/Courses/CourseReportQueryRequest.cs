using Swashbuckle.AspNetCore.Annotations;

namespace BusinessObject.DTOs.Request.Courses
{
    public class CourseReportQueryRequest
    {
        [SwaggerSchema("Số trang (mặc định: 1)")]
        public int Page { get; set; } = 1;

        [SwaggerSchema("Số lượng item mỗi trang (mặc định: 10)")]
        public int PageSize { get; set; } = 10;

        [SwaggerSchema("Lọc theo status: PROCESSING, COMPLETED")]
        public string? Status { get; set; }

        [SwaggerSchema("Lọc theo loại report (ví dụ: 'nội dung không phù hợp', 'trùng lặp')")]
        public string? ReportType { get; set; }

        [SwaggerSchema("Tìm kiếm theo tiêu đề report")]
        public string? SearchByTitle { get; set; }
    }
}
