using BusinessObject.DTOs.Response.MockTestQuestion;

namespace BusinessObject.DTOs.Response.MockTest
{
    public class MockTestResultResponse
    {
        public int MockTestId { get; set; }

        public string MockTestTitle { get; set; }

        public string MockTestDescription { get; set; }

        public DateTime CreatedAt { get; set; }

        public DateTime UpdatedAt { get; set; }

        public string CreatedBy { get; set; }

        public List<MockTestResultQuestionResponse> Questions { get; set; } = new List<MockTestResultQuestionResponse>();
    }
}
