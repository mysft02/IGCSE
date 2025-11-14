using Swashbuckle.AspNetCore.Annotations;

namespace BusinessObject.DTOs.Request.Courses
{
    public class ParentEnrollmentQueryRequest
    {
        [SwaggerSchema("Số trang (bắt đầu từ 0)")]
        public int Page { get; set; } = 0;

        [SwaggerSchema("Số lượng item mỗi trang")]
        public int Size { get; set; } = 10;

        [SwaggerSchema("Tìm kiếm theo tên học sinh")]
        public string? SearchByStudentName { get; set; }

        [SwaggerSchema("Tìm kiếm theo tên khóa học")]
        public string? SearchByCourseName { get; set; }

        [SwaggerSchema("Filter theo Student ID")]
        public string? StudentId { get; set; }

        [SwaggerSchema("Filter theo Course ID")]
        public long? CourseId { get; set; }

        [SwaggerSchema("Filter theo ngày đăng ký từ")]
        public DateTime? EnrolledFrom { get; set; }

        [SwaggerSchema("Filter theo ngày đăng ký đến")]
        public DateTime? EnrolledTo { get; set; }

        public int GetPageSize()
        {
            return Size > 0 ? Size : 10;
        }
    }
}

