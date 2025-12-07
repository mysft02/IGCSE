using BusinessObject.DTOs.Response.Modules;

namespace BusinessObject.DTOs.Response.Courses
{
    public class CourseResponse
    {
        public int CourseId { get; set; }

        public string Name { get; set; } = string.Empty;

        public string? Description { get; set; } = string.Empty;

        public string Status { get; set; } = string.Empty;

        public decimal Price { get; set; }

        public string? ImageUrl { get; set; } = string.Empty;

        public DateTime? CreatedAt { get; set; }

        public DateTime? UpdatedAt { get; set; }

        public ModuleResponse Module { get; set; }

        public string CreatedBy { get; set; }
    }
}
