namespace BusinessObject.Model;

public class Quizresult
{
    public int QuizResultId { get; set; }

    public int QuizId { get; set; }

    public bool IsPassed { get; set; }

    public decimal Score { get; set; }

    public DateTime CreatedAt { get; set; }

    public string UserId { get; set; } = null!;
}
