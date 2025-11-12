using BusinessObject.Model;

namespace BusinessObject.DTOs.Response.MockTest
{
    public class MockTestCreateResponse
    {
        public string Title { get; set; }

        public string Description { get; set; }

        public List<Mocktestquestion> Questions { get; set; } = new List<Mocktestquestion>();
    }
}
