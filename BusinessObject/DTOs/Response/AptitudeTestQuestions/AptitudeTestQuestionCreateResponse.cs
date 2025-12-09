namespace BusinessObject.DTOs.Response.AptitudeTestQuestionResponse
{
    public class AptitudeTestQuestionCreateResponse
    {
        public List<AptitudeQuestionResponse> Questions { get; set; }
    }

    public class AptitudeQuestionResponse
    {
        public int AptitudeTestQuestionId { get; set; }

        public string? QuestionContent { get; set; }

        public string? Answer { get; set; }

        public string? ModuleId { get; set; }
    }
}
