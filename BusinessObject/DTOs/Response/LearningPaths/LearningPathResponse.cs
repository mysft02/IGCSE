using BusinessObject.DTOs.Response.Courses;
using BusinessObject.DTOs.Response.Modules;
using BusinessObject.Enums;
using BusinessObject.Model;

namespace BusinessObject.DTOs.Response.LearningPaths
{
    public class LearningPathResponse
    {
        public int LearningPathId { get; set; }

        public string Title { get; set; } = null!;

        public string Description { get; set; } = null!;

        public List<ModuleWithCourseResponse>? ModuleResponses { get; set; }
    }

    public class ModuleWithCourseResponse
    {
        public int ModuleId { get; set; }
        public string ModuleName { get; set; }
        public string Description { get; set; }

        // This will be mapped from Module.CourseSubject
        public CourseSubject CourseSubject { get; set; }

        // Computed property to get the display name
        public string CourseSubjectName =>
            CourseSubjectHelper.GetDisplayName(CourseSubject);

        public IEnumerable<CourseModuleResponse>? Course {  get; set; }
    }

    public class CourseModuleResponse
    {
        public int CourseId { get; set; }

        public string Name { get; set; } = string.Empty;

        public string? Description { get; set; } = string.Empty;

        public decimal Price { get; set; }

        public string? ImageUrl { get; set; } = string.Empty;

        public string CreatedBy { get; set; }
    }
}
