using Microsoft.AspNetCore.Http;

namespace BusinessObject.DTOs.Request.TeacherProfiles
{
    public class TeacherProfileCreateRequest
    {
        public string? TeacherName { get; set; } = null!;

        public string? Description { get; set; } = null!;

        public IFormFile? Avatar { get; set; } = null!;

        public int? Experience { get; set; }
    }
}
