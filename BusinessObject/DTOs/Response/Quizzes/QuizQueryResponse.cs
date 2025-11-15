using BusinessObject.Model;

namespace BusinessObject.DTOs.Response.Quizzes
{
    public class QuizQueryResponse
    {
        public int QuizId { get; set; }

        public string QuizTitle { get; set; }

        public string QuizDescription { get; set; }

        public int LessonId { get; set; }
    }
}
