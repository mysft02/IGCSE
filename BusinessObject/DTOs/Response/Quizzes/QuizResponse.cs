using BusinessObject.DTOs.Response.FinalQuizzes;

namespace BusinessObject.DTOs.Response.Quizzes
{
    public class QuizResponse
    {
        public int Id { get; set; }

        public string Title { get; set; }

        public string Description { get; set; }

        public List<FinalQuizQuestionResponse> Questions { get; set; }
    }
}
