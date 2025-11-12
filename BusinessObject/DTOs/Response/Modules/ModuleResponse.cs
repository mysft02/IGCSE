using System;
using BusinessObject.Enums;

namespace BusinessObject.DTOs.Response.Modules
{
    public class ModuleResponse
    {
        public int ModuleId { get; set; }
        public string ModuleName { get; set; }
        public string Description { get; set; }
        public bool IsActive { get; set; }
        public DateTime? CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }
        
        // This will be mapped from Module.CourseSubject
        public CourseSubject CourseSubject { get; set; }
        
        // Computed property to get the display name
        public string CourseSubjectName => 
            CourseSubjectHelper.GetDisplayName(CourseSubject);
    }
}
