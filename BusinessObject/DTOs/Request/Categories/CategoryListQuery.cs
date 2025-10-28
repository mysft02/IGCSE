using Swashbuckle.AspNetCore.Annotations;

namespace BusinessObject.DTOs.Request.Categories
{
    public class CategoryListQuery
    {
        public int Page { get; set; } = 1;

        public int PageSize { get; set; } = 10;

        [SwaggerSchema("Tên category để tìm kiếm (nếu có)")]
        public string? SearchByName { get; set; } = null;

        [SwaggerSchema("Trạng thái active để lọc (nếu có)")]
        public bool? IsActive { get; set; } = null;
    }
}



