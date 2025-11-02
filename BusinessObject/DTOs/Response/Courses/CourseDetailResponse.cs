using BusinessObject.DTOs.Response.CourseContent;

namespace BusinessObject.DTOs.Response.Courses
{
    public class CourseDetailResponse
    {
        public long CourseId { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public string Status { get; set; } = string.Empty;
        public decimal Price { get; set; }
        public string ImageUrl { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
        public int CategoryId { get; set; }
        public string CategoryName { get; set; } = string.Empty;

        // Thông tin chi tiết về cấu trúc khóa học
        public List<CourseSectionDetailResponse> Sections { get; set; } = new List<CourseSectionDetailResponse>();
    }

    public class CourseSectionDetailResponse
    {
        public long CourseSectionId { get; set; }
        public long CourseId { get; set; }
        public string Name { get; set; } = string.Empty;
        public string? Description { get; set; }
        public int Order { get; set; }
        public bool IsActive { get; set; }
        public List<LessonDetailResponse> Lessons { get; set; } = new List<LessonDetailResponse>();
    }

    public class LessonDetailResponse
    {
        public long LessonId { get; set; }
        public long CourseSectionId { get; set; }
        public string Name { get; set; } = string.Empty;
        public string? Description { get; set; }
        public int Order { get; set; }
        public bool IsActive { get; set; }
        public List<LessonItemResponse> LessonItems { get; set; } = new List<LessonItemResponse>();
    }
}
