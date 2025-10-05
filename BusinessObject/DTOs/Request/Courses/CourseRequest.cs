using System.ComponentModel.DataAnnotations;

namespace DTOs.Request.Courses
{
    public class CourseRequest
    {
        [Required]
        [StringLength(200)]
        public string CourseName { get; set; } = string.Empty;

        [StringLength(1000)]
        public string Description { get; set; } = string.Empty;

        [Range(0, 2)]
        public BusinessObject.Model.CourseStatus Status { get; set; } = BusinessObject.Model.CourseStatus.Draft;

        [Range(0, double.MaxValue)]
        public decimal Price { get; set; }

        public string ImageUrl { get; set; } = string.Empty;

        [Required]
        public int CategoryID { get; set; }
    }
}
