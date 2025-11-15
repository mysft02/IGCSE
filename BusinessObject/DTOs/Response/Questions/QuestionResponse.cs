using System.Text.Json.Serialization;

namespace BusinessObject.DTOs.Response.Questions
{
    public class QuestionResponse
    {
        public int QuestionId { get; set; }

        public string QuestionContent { get; set; } = null!;

        public string CorrectAnswer { get; set; } = null!;

        public DateTime CreatedAt { get; set; }

        public DateTime UpdatedAt { get; set; }

        [JsonIgnore]
        public string? PictureUrl { get; set; }

        public string? Image { get; set; }
    }
}
