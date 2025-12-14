namespace BusinessObject.DTOs.Response.MockTest
{
    public class MockTestWithAnswerResponse
    {
        public int MockTestId { get; set; }

        public string MockTestTitle { get; set; }

        public string MockTestDescription { get; set; }

        public DateTime CreatedAt { get; set; }

        public List<MockTestQuestionWithAnswerResponse> Questions { get; set; }
    }

    public class MockTestQuestionWithAnswerResponse
    {
        public int QuestionId { get; set; }

        public string QuestionContent { get; set; } = null!;

        public string CorrectAnswer { get; set; } = null!;

        public string? ImageUrl { get; set; } = null!;
    }
}
