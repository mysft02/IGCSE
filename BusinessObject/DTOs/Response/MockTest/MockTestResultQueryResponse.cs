using BusinessObject.Model;

namespace BusinessObject.DTOs.Response.MockTest
{
    public class MockTestResultQueryResponse
    {
        public Mocktest MockTest { get; set; }

        public decimal Score { get; set; }

        public DateTime DateTaken { get; set; }
    }
}
