using BusinessObject.Model;

namespace BusinessObject.DTOs.Response.MockTest
{
    public class MockTestResultQueryResponse
    {
        public int MockTestResultId { get; set; }

        public MockTestResultResponse MockTest { get; set; }

        public decimal Score { get; set; }

        public DateTime DateTaken { get; set; }
    }
}
