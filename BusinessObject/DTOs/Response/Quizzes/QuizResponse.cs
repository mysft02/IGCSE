using BusinessObject.Model;
namespace BusinessObject.DTOs.Response.Quizzes
{
    public class QuizResponse
    {
        public int Id { get; set; }

        public string Title { get; set; }

        public string Description { get; set; }

        public List<Question> Questions { get; set; }
    }
}
