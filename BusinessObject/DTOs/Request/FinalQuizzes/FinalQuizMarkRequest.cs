using BusinessObject.DTOs.Request.UserAnswers;

namespace BusinessObject.DTOs.Request.FinalQuizzes
{
    public class FinalQuizMarkRequest
    {
        public int FinalQuizID { get; set; }
        public List<UserAnswerRequest> UserAnswers { get; set; }
    }
}
