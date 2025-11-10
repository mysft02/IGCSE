using System;

namespace BusinessObject.DTOs.Response.CourseRegistration
{
    public class CourseRegistrationResponse
    {
        public int CourseId { get; set; }
        public string CourseName { get; set; } = string.Empty;
        public string StudentId { get; set; } = string.Empty;
        public string StudentName { get; set; } = string.Empty;
        public DateTime EnrollmentDate { get; set; }
        public string Status { get; set; } = string.Empty;
    }
}
