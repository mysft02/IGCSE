using System;

namespace BusinessObject.DTOs.Response.CourseRegistration
{
    public class CourseRegistrationResponse
    {
        public int CourseId { get; set; }

        public string CourseName { get; set; } = string.Empty;

        public string CourseDescription { get; set; }

        public string? ImageUrl { get; set; }

        public string CreatedBy { get; set; }

        public DateTime EnrollmentDate { get; set; }

        public string Status { get; set; } = string.Empty;
    }
}
