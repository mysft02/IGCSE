namespace BusinessObject.Model;

public class Quizuseranswer
{
    public int QuizUserAnswerId { get; set; }

    public int QuestionId { get; set; }

    public string? Answer { get; set; }

    public int QuizId { get; set; }
}
