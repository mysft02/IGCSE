using BusinessObject.Model;namespace BusinessObject.DTOs.Response.MockTest
{
    public class MockTestResponse
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
