using Swashbuckle.AspNetCore.Annotations;

namespace BusinessObject.DTOs.Request.Courses
{
    public class TeacherCourseQueryRequest
    {
        [SwaggerSchema("Số trang (bắt đầu từ 0)")]
        public int Page { get; set; } = 0;

        [SwaggerSchema("Số lượng item mỗi trang")]
        public int Size { get; set; } = 10;

        [SwaggerSchema("Tìm kiếm theo tên khóa học")]
        public string? SearchByCourseName { get; set; }

        public int GetPageSize()
        {
            return Size > 0 ? Size : 10;
        }
    }
}


