using System.ComponentModel.DataAnnotations;
using BusinessObject.Enums;

namespace BusinessObject.DTOs.Request.Modules
{
    public class ModuleRequest
    {
        [Required]
        public string ModuleName { get; set; }
        
        public string Description { get; set; }
        
        public bool IsActive { get; set; } = true;
        
        [Required]
        public CourseSubject CourseSubject { get; set; }
    }
}
