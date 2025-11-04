using BusinessObject.DTOs.Request.UserAnswers;

namespace BusinessObject.DTOs.Request.MockTest
{
    public class MockTestMarkRequest
    {
        public int MockTestId { get; set; }
        public List<UserAnswerRequest> UserAnswers { get; set; }
    }
}
