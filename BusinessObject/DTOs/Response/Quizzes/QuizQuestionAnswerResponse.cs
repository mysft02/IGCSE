namespace BusinessObject.DTOs.Response.Quizzes
{
    public class QuizQuestionAnswerResponse
    {
        public int QuestionId { get; set; }

        public string? QuestionContent { get; set; }

        public string? ImageUrl { get; set; }

        public string? CorrectAnswer { get; set; }
    }
}
