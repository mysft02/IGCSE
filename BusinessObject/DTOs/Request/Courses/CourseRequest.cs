using Microsoft.AspNetCore.Http;
using System.ComponentModel.DataAnnotations;

namespace DTOs.Request.Courses
{
    public class CourseRequest
    {
        [Required]
        [StringLength(200)]
        public string Name { get; set; } = string.Empty;

        [StringLength(1000)]
        public string Description { get; set; } = string.Empty;

        [Range(0, double.MaxValue)]
        public decimal Price { get; set; }

        public string ImageUrl { get; set; } = string.Empty;

        [Required]
        public int CategoryId { get; set; }

        [Required]
        public string Status { get; set; } = string.Empty;

        public IFormFile? ImageFile { get; set; }
    }
}
