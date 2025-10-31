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
        public List<ModuleDetailResponse> Modules { get; set; } = new();
    }
    public class ModuleDetailResponse
    {
        public int ModuleID { get; set; }
        public string ModuleName { get; set; } = string.Empty;
        public string? Description { get; set; }
        public sbyte IsActive { get; set; }
        public DateTime? CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }
        public List<ChapterDetailResponse> Chapters { get; set; } = new();
    }
    public class ChapterDetailResponse
    {
        public int ChapterID { get; set; }
        public int ModuleID { get; set; }
        public string ChapterName { get; set; } = string.Empty;
        public string? ChapterDescription { get; set; }
        public DateTime? CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }
        public List<CourseSectionDetailResponse> Sections { get; set; } = new();
    }
    public class CourseSectionDetailResponse
    {
        public long CourseSectionId { get; set; }
        public long CourseId { get; set; }
        public string Name { get; set; } = string.Empty;
        public string? Description { get; set; }
        public int Order { get; set; }
        public bool IsActive { get; set; }
        public List<LessonDetailResponse> Lessons { get; set; } = new();
    }
    public class LessonDetailResponse
    {
        public long LessonId { get; set; }
        public long CourseSectionId { get; set; }
        public string Name { get; set; } = string.Empty;
        public string? Description { get; set; }
        public int Order { get; set; }
        public bool IsActive { get; set; }
        public List<LessonItemResponse> LessonItems { get; set; } = new();
    }
}
