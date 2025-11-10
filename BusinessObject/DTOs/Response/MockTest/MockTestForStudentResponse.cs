using BusinessObject.DTOs.Response.MockTestQuestion;

namespace BusinessObject.DTOs.Response.MockTest
{
    public class MockTestForStudentResponse
    {
        public int MockTestId { get; set; }

        public string MockTestTitle { get; set; }

        public string MockTestDescription { get; set; }

        public DateTime CreatedAt { get; set; }

        public DateTime UpdatedAt { get; set; }

        public string CreatedBy { get; set; }

        public List<MockTestQuestionResponse> MockTestQuestions { get; set; } = new List<MockTestQuestionResponse>();
    }
}
