using System;

namespace DTOs.Response.CourseRegistration
{
    public class CourseRegistrationResponse
    {
        public long CourseKeyId { get; set; }
        public long CourseId { get; set; }
        public string CourseName { get; set; } = string.Empty;
        public string CategoryName { get; set; } = string.Empty;
        public string StudentId { get; set; } = string.Empty;
        public string StudentName { get; set; } = string.Empty;
        public string CourseKey { get; set; } = string.Empty;
        public DateTime EnrollmentDate { get; set; }
        public string Status { get; set; } = string.Empty;
    }
}
