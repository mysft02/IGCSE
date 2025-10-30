using System.Text.Json.Serialization;

namespace BusinessObject.DTOs.Response.MockTestQuestion
{
    public class MockTestQuestionResponse
    {
        public int MockTestQuestionId { get; set; }

        public string QuestionContent { get; set; } = null!;

        public string CorrectAnswer { get; set; } = null!;

        public string? Image { get; set; }

        [JsonIgnore] 
        public string? ImageUrl { get; set; }

        public DateTime CreatedAt { get; set; }

        public DateTime UpdatedAt { get; set; }
    }
}
