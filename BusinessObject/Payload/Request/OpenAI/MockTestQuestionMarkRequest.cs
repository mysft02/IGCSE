namespace BusinessObject.Payload.Request.OpenAI
{
    public class MockTestQuestionMarkRequest
    {
        public string QuestionText { get; set; }

        public string? Answer { get; set; }

        public string RightAnswer { get; set; }

        public decimal Mark { get; set; }

        public string? PartialMark { get; set; }

        public string? ImageBase64 { get; set; }

        public int QuestionId { get; set; }
    }
}
