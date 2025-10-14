namespace BusinessObject.DTOs.Request.UserAnswers
{
    public class UserAnswerRequest
    {
        public int QuestionId { get; set; }
        public string Answer { get; set; }
        public string UserId { get; set; }
    }
}
