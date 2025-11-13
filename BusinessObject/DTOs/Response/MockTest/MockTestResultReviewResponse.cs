
namespace BusinessObject.DTOs.Response.MockTest
{
    public class MockTestResultReviewResponse
    {
        public int MockTestResultId { get; set; }

        public MockTestResultReviewDetailResponse MockTest { get; set; }

        public decimal Score { get; set; }

        public DateTime DateTaken { get; set; }
    }
}
