using Microsoft.AspNetCore.Http;
using Swashbuckle.AspNetCore.Annotations;
using System.ComponentModel.DataAnnotations;

namespace BusinessObject.DTOs.Request.Courses
{
    public class CourseRequest
    {
        [Required]
        [StringLength(200)]
        [SwaggerSchema("Tên khóa học", Nullable = false)]
        public string Name { get; set; } = string.Empty;

        [StringLength(1000)]
        [SwaggerSchema("Mô tả khóa học", Nullable = false)]
        public string Description { get; set; } = string.Empty;

        [Range(0, double.MaxValue)]
        [SwaggerSchema("Giá mua khóa học", Nullable = false)]
        public decimal Price { get; set; }

        [SwaggerSchema("Link hình ảnh khóa học", Nullable = false, Description = "Nếu chọn hình ảnh từ file thì chọn send null")]
        public string ImageUrl { get; set; } = string.Empty;

        [Required]
        [SwaggerSchema("Module ID của khóa học", Nullable = false)]
        public int ModuleId { get; set; }

        [Required]
        [SwaggerSchema("Trạng thái khóa học", Nullable = false, Description ="Mặc định tạo là Pending")]
        public string Status { get; set; } = string.Empty;

        [SwaggerSchema("Hình ảnh khóa học", Nullable = false, Description = "Nếu chọn hình ảnh bằng link thì chọn send null")]
        public IFormFile? ImageFile { get; set; }
    }
}
