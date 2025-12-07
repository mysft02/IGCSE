using BusinessObject.DTOs.Response.Courses;
using BusinessObject.Model;

namespace BusinessObject.DTOs.Request.Courses
{
    public class SimilarCourseRequest
    {
        public int CourseId { get; set; }
    }

    public class SimilarCourseForStudentRequest
    {
        public Account User { get; set; }

        public List<CourseWithFinalQuizResultResponse> Courses { get; set; }
    }

    public class CourseWithFinalQuizResultResponse
    {
        public CourseWithStudyingTimeResponse Course { get; set; }

        public List<FinalQuizResultResponse> Finalquizresults { get; set; }
    }

    public class FinalQuizResultResponse
    {
        public int FinalQuizResultId { get; set; }

        public decimal Score { get; set; }

        public bool IsPassed { get; set; }

        public DateTime CreatedAt { get; set; }
    }
}
