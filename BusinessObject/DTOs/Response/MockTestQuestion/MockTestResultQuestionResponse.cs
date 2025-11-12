namespace BusinessObject.DTOs.Response.MockTestQuestion
{
    public class MockTestResultQuestionResponse
    {
        public int QuestionId { get; set; }

        public string QuestionContent { get; set; } = null!;

        public string CorrectAnswer { get; set; } = null!;

        public string ImageUrl { get; set; } = null!;

        public decimal? Mark { get; set; }

        public string? PartialMark { get; set; }

        public MockTestQuestionUserAnswerResponse UserAnswer { get; set; }
    }
}
