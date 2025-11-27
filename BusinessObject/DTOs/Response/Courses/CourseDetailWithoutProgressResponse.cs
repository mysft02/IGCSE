using BusinessObject.DTOs.Response.FinalQuizzes;

namespace BusinessObject.DTOs.Response.Courses
{
    public class CourseDetailWithoutProgressResponse
    {
        public long CourseId { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public string Status { get; set; } = string.Empty;
        public decimal Price { get; set; }
        public string ImageUrl { get; set; } = string.Empty;
        public DateTime? CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }
        public List<CourseSectionDetailWithoutProgressResponse> Sections { get; set; } = new();
    }

    public class CourseSectionDetailWithoutProgressResponse
    {
        public long CourseSectionId { get; set; }
        public string Name { get; set; } = string.Empty;
        public string? Description { get; set; }
        public int Order { get; set; }
        public bool IsActive { get; set; }
        public List<LessonDetailWithoutProgressResponse> Lessons { get; set; } = new();
    }

    public class LessonDetailWithoutProgressResponse
    {
        public long LessonId { get; set; }
        public string Name { get; set; } = string.Empty;
        public string? Description { get; set; }
        public int Order { get; set; }
        public bool IsActive { get; set; }
        public List<LessonItemDetailWithoutProgressResponse> LessonItems { get; set; } = new();
        public LessonQuizResponse Quiz { get; set; }
    }

    public class LessonItemDetailWithoutProgressResponse
    {
        public long LessonItemId { get; set; }
        public string Name { get; set; } = string.Empty;
        public int Order { get; set; }
    }
}
