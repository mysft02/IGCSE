using System;

namespace DTOs.Response.Courses
{
    public class CourseResponse
    {
        public int CourseID { get; set; }
        public string CourseName { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public string Status { get; set; }
        public decimal Price { get; set; }
        public string ImageUrl { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
        public int CategoryID { get; set; }
        public string CategoryName { get; set; } = string.Empty;
    }
}
