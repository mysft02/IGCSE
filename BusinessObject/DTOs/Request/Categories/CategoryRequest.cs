using Swashbuckle.AspNetCore.Annotations;
using System.ComponentModel.DataAnnotations;

namespace DTOs.Request.Categories
{
    public class CategoryRequest
    {
        [Required]
        [StringLength(200)]
        [SwaggerSchema("Tên category", Nullable = false)]
        public string CategoryName { get; set; } = string.Empty;

        [StringLength(4000)]
        [SwaggerSchema("Mô tả category", Nullable = false)]
        public string Description { get; set; } = string.Empty;

        [SwaggerSchema("Trạng thái category", Nullable = false, Description = "true là active, false là inactive")]
        public bool IsActive { get; set; } = true;
    }
}
