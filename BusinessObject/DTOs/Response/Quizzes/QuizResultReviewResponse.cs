namespace BusinessObject.DTOs.Response.Quizzes
{
    public class QuizResultReviewResponse
    {
        public int QuizResultId { get; set; }

        public QuizResultReviewDetailResponse Quiz { get; set; }

        public decimal Score { get; set; }

        public bool IsPassed { get; set; }

        public DateTime DateTaken { get; set; }
    }

    public class QuizResultReviewDetailResponse
    {
        public int QuizId { get; set; }

        public string Title { get; set; }

        public string Description { get; set; }

        public List<QuizResultQuestionResponse> Questions { get; set; } = new List<QuizResultQuestionResponse>();
    }

    public class QuizResultQuestionResponse
    {
        public int QuestionId { get; set; }

        public string QuestionContent { get; set; }

        public string? ImageUrl { get; set; }

        public string? CorrectAnswer { get; set; }

        public QuizQuestionUserAnswerResponse? UserAnswer { get; set; }
    }

    public class QuizQuestionUserAnswerResponse
    {
        public int QuizUserAnswerId { get; set; }

        public string? UserAnswer { get; set; }
    }
}

