using BusinessObject.Model;

namespace BusinessObject.DTOs.Response.Quizzes
{
    public class QuizMarkResponse
    {
        public string Question { get; set; }

        public string Answer { get; set; }

        public string RightAnswer { get; set; }

        public bool IsCorrect { get; set; }

        public string Comment { get; set; }
    }
}
