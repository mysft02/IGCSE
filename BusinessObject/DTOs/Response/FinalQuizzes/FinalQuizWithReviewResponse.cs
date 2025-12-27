using BusinessObject.DTOs.Response.Quizzes;

namespace BusinessObject.DTOs.Response.FinalQuizzes
{
    public class FinalQuizWithReviewResponse
    {
        public int Id { get; set; }

        public string Title { get; set; }

        public string Description { get; set; }

        public bool IsPassed { get; set; }

        public decimal Score { get; set; }

        public DateTime DateTaken { get; set; }

        public List<QuizResultQuestionResponse> Questions { get; set; }
    }
}
