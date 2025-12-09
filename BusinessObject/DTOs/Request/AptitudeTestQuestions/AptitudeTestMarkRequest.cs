using BusinessObject.DTOs.Request.UserAnswers;

namespace BusinessObject.DTOs.Request.AptitudeTestQuestions
{
    public class AptitudeTestMarkRequest
    {
        public List<UserAnswerRequest> UserAnswers { get; set; }
    }

    public class AptitudeTestQuestionMarkRequest
    {
        public int ModuleId { get; set; }

        public string QuestionText { get; set; }

        public string Answer { get; set; }

        public string RightAnswer { get; set; }
    }
}
