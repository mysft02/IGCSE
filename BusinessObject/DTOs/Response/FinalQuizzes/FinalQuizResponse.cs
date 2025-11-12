using BusinessObject.Model;

namespace BusinessObject.DTOs.Response.FinalQuizzes
{
    public class FinalQuizResponse
    {
        public int Id { get; set; }

        public string Title { get; set; }

        public string Description { get; set; }

        public List<FinalQuizQuestionResponse> Questions { get; set; }
    }
}
