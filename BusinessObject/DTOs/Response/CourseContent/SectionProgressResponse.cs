using System.Collections.Generic;

namespace DTOs.Response.CourseContent
{
    public class SectionProgressResponse
    {
        public long CourseSectionId { get; set; }
        public string SectionName { get; set; } = string.Empty;
        public int Order { get; set; }
        public bool IsActive { get; set; }
        public List<LessonProgressResponse> Lessons { get; set; } = new List<LessonProgressResponse>();
    }
}
