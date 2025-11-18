using BusinessObject.DTOs.Response.CourseContent;
using BusinessObject.DTOs.Response.Quizzes;

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
        public List<CourseSectionDetailResponse> Sections { get; set; } = new();
        
        // Thông tin tiến trình học (chỉ có khi student đã đăng nhập và enroll)
        public bool IsEnrolled { get; set; } = false;
        public double? OverallProgress { get; set; } = null; // Phần trăm hoàn thành (0-100)
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
        public long CourseId { get; set; }
        public long CourseSectionId { get; set; }
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
        public bool IsUnlocked { get; set; } = false;  // Trạng thái mở khóa bài học
        public bool IsCompleted { get; set; } = false; // Trạng thái hoàn thành bài học
        public List<LessonItemDetailResponse> LessonItems { get; set; } = new();
        public QuizResponse Quiz { get; set; }
    }
    
    public class LessonItemDetailResponse
    {
        public long LessonItemId { get; set; }
        public string Name { get; set; } = string.Empty;
        public int Order { get; set; }
        public bool IsCompleted { get; set; } = false; // Trạng thái hoàn thành item
        public DateTime? CompletedAt { get; set; } // Thời gian hoàn thành
    }

    public class LessonItemDetail
    {
        public long LessonItemId { get; set; }
        public string Name { get; set; } = string.Empty;
        public string? Description { get; set; }
        public string Content { get; set; } = string.Empty;
        public string ItemType { get; set; } = string.Empty; // video, text, quiz, etc.
    }
}
