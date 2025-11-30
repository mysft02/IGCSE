using Swashbuckle.AspNetCore.Annotations;

namespace BusinessObject.DTOs.Request.Courses
{
    public class StudentProgressQueryRequest
    {
        [SwaggerSchema("Số trang (bắt đầu từ 0)")]
        public int Page { get; set; } = 0;

        [SwaggerSchema("Số lượng item mỗi trang")]
        public int Size { get; set; } = 10;

        [SwaggerSchema("Tìm kiếm theo tên học sinh")]
        public string? SearchByStudentName { get; set; }

        [SwaggerSchema("Filter theo Student ID")]
        public string? StudentId { get; set; }

        [SwaggerSchema("Filter theo Course ID")]
        public int? CourseId { get; set; }

        [SwaggerSchema("Filter theo progress range (min)")]
        public double? MinProgress { get; set; }

        [SwaggerSchema("Filter theo progress range (max)")]
        public double? MaxProgress { get; set; }

        public int GetPageSize()
        {
            return Size > 0 ? Size : 10;
        }
    }
}


