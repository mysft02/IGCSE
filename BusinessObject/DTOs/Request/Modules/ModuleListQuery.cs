using BusinessObject.Enums;
using Swashbuckle.AspNetCore.Annotations;

namespace BusinessObject.DTOs.Request.Modules
{
    public class ModuleListQuery
    {
        public int Page { get; set; } = 1;

        public int PageSize { get; set; } = 10;

        [SwaggerSchema("Tìm theo tên module")]
        public string? SearchByName { get; set; } = null;

        [SwaggerSchema("Lọc theo môn toán")]
        public CourseSubject? CourseSubject { get; set; } = null;

        [SwaggerSchema("Lọc theo trạng thái hoạt động")]
        public bool? IsActive { get; set; } = null;
    }
}

