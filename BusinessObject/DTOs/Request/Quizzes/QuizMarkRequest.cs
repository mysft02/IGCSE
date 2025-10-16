using BusinessObject.DTOs.Request.UserAnswers;

namespace BusinessObject.DTOs.Request.Quizzes
{
    public class QuizMarkRequest
    {
        public List<UserAnswerRequest> UserAnswers { get; set; }
    }
}
