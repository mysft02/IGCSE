using BusinessObject.Model;

namespace BusinessObject.Payload.Response.MockTest
{
    public class MockTestQueryResponse
    {
        public int MockTestId { get; set; }
        
        public string MockTestTitle { get; set; }

        public string MockTestDescription { get; set; }

        public DateTime CreatedAt { get; set; }

        public DateTime UpdatedAt { get; set; }

        public string CreatedBy { get; set; }

        public List<Mocktestquestion> MockTestQuestions { get; set; }
    }
}
