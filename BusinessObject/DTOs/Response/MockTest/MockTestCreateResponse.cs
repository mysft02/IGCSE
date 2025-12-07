namespace BusinessObject.DTOs.Response.MockTest
{
    public class MockTestCreateResponse
    {
        public int MockTestId { get; set; }

        public string MockTestTitle { get; set; }

        public string MockTestDescription { get; set; }

        public DateTime CreatedAt { get; set; }

        public DateTime UpdatedAt { get; set; }

        public List<MockTestQuestionCreateResponse> MockTestQuestions { get; set; } = new List<MockTestQuestionCreateResponse>(); // <MockTestQuestion>
    }

    public class MockTestQuestionCreateResponse
    {
        public int MockTestQuestionId { get; set; }

        public int MockTestId { get; set; }

        public string QuestionContent { get; set; }

        public string CorrectAnswer { get; set; }

        public string PartialMark { get; set; }

        public decimal Mark { get; set; }
    }
}
