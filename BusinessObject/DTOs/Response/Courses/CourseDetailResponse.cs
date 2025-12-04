using BusinessObject.DTOs.Response.FinalQuizzes;

namespace BusinessObject.DTOs.Response.Courses
{
    public class CourseDetailResponse
    {
        public int CourseId { get; set; }

        public string Name { get; set; } = string.Empty;

        public string Description { get; set; } = string.Empty;

        public string Status { get; set; } = string.Empty;

        public decimal Price { get; set; }

        public string ImageUrl { get; set; } = string.Empty;

        public DateTime? CreatedAt { get; set; }

        public DateTime? UpdatedAt { get; set; }

        public string CreatedBy { get; set; } 

        public List<CourseSectionDetailResponse> Sections { get; set; } = new();
        
        // Thông tin tiến trình học (chỉ có khi student đã đăng nhập và enroll)
        public bool IsEnrolled { get; set; } = false;

        public double? OverallProgress { get; set; } = null; // Phần trăm hoàn thành (0-100)

        public FinalQuizCourseDetailResponse FinalQuiz { get; set; }

        public CourseCertificateResponse Certificate { get; set; }
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
        public int CourseId { get; set; }
        public int CourseSectionId { get; set; }
        public string Name { get; set; } = string.Empty;
        public string? Description { get; set; }
        public int Order { get; set; }
        public bool IsActive { get; set; }
        public List<LessonDetailResponse> Lessons { get; set; } = new();
    }
    public class LessonDetailResponse
    {
        public int LessonId { get; set; }
        public int CourseSectionId { get; set; }
        public string Name { get; set; } = string.Empty;
        public string? Description { get; set; }
        public int Order { get; set; }
        public bool IsActive { get; set; }
        public bool IsUnlocked { get; set; } = false;  // Trạng thái mở khóa bài học
        public bool IsCompleted { get; set; } = false; // Trạng thái hoàn thành bài học
        public List<LessonItemDetailResponse> LessonItems { get; set; } = new();
        public LessonQuizResponse Quiz { get; set; }
    }

    public class LessonQuizResponse
    {
        public int QuizId { get; set; }
        public string QuizTitle { get; set; }
        public string QuizDescription { get; set; }
    }
    
    public class LessonItemDetailResponse
    {
        public int LessonItemId { get; set; }
        public string Name { get; set; } = string.Empty;
        public int Order { get; set; }
        public bool IsCompleted { get; set; } = false; // Trạng thái hoàn thành item
        public DateTime? CompletedAt { get; set; } // Thời gian hoàn thành
    }

    public class LessonItemDetail
    {
        public int LessonItemId { get; set; }
        public string Name { get; set; } = string.Empty;
        public string? Description { get; set; }
        public string Content { get; set; } = string.Empty;
        public string ItemType { get; set; } = string.Empty; // video, text, quiz, etc.
    }
}
