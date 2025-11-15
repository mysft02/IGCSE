namespace BusinessObject.DTOs.Response.ParentStudentLink
{
    public class ParentEnrollmentResponse
    {
        public int EnrollmentId { get; set; }
        public string StudentId { get; set; } = string.Empty;
        public string StudentName { get; set; } = string.Empty;
        public int CourseId { get; set; }
        public string CourseName { get; set; } = string.Empty;
        public decimal CoursePrice { get; set; }
        public DateTime EnrolledAt { get; set; }
    }
}


