namespace BusinessObject.DTOs.Request.Modules
{
    public class ModuleRequest
    {
        public int CourseId { get; set; }
        public string ModuleName { get; set; } = string.Empty;
        public string? Description { get; set; }
        public sbyte IsActive { get; set; }
    }
}
