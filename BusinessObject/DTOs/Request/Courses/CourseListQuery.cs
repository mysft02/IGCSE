using Swashbuckle.AspNetCore.Annotations;

namespace BusinessObject.DTOs.Request.Courses
{
    public class CourseListQuery
    {
        public int Page { get; set; } = 1;

        public int PageSize { get; set; } = 10;

        [SwaggerSchema("Tên khóa học để tìm kiếm(nếu có)")]
        public string? SearchByName { get; set; } = null;

        [SwaggerSchema("Id khóa học để tìm kiếm (nếu có)")]
        public long? CouseId { get; set; } = null; // filter by specific course id

        [SwaggerSchema("Trạng thái khóa học để tìm kiếm (trạng thái: 1 là Pending(chưa duyệt); 2 là Open())")]
        public string? Status { get; set; } = null;
    }
}
