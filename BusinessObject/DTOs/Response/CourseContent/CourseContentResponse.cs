using System.Collections.Generic;

namespace DTOs.Response.CourseContent
{
    public class CourseSectionResponse
    {
        public long CourseSectionId { get; set; }
        public long CourseId { get; set; }
        public string Name { get; set; } = string.Empty;
        public string? Description { get; set; }
        public int Order { get; set; }
        public bool IsActive { get; set; }
        public List<LessonResponse> Lessons { get; set; } = new List<LessonResponse>();
    }

    public class LessonResponse
    {
        public long LessonId { get; set; }
        public long CourseSectionId { get; set; }
        public string Name { get; set; } = string.Empty;
        public string? Description { get; set; }
        public int Order { get; set; }
        public bool IsActive { get; set; }
    }

    public class LessonItemResponse
    {
        public long LessonItemId { get; set; }
        public string Name { get; set; } = string.Empty;
        public string? Description { get; set; }
        public string Content { get; set; } = string.Empty;
        public string ItemType { get; set; } = string.Empty; // video, text, quiz, etc.
        public int Order { get; set; }
    }

    public class StudentProgressResponse
    {
        public long CourseKeyId { get; set; }
        public long CourseId { get; set; }
        public string CourseName { get; set; } = string.Empty;
        public string StudentName { get; set; } = string.Empty;
        public List<LessonProgressResponse> LessonProgress { get; set; } = new List<LessonProgressResponse>();
        public double OverallProgress { get; set; }
    }

    public class LessonProgressResponse
    {
        public long LessonId { get; set; }
        public string LessonName { get; set; } = string.Empty;
        public bool IsCompleted { get; set; }
        public DateTime? CompletedAt { get; set; }
        public List<LessonItemProgressResponse> ItemProgress { get; set; } = new List<LessonItemProgressResponse>();
    }

    public class LessonItemProgressResponse
    {
        public long LessonItemId { get; set; }
        public string LessonItemName { get; set; } = string.Empty;
        public bool IsCompleted { get; set; }
        public DateTime? CompletedAt { get; set; }
    }
}
