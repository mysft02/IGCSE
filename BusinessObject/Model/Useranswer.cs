namespace BusinessObject.Model;

public class Useranswer
{
    public int UserAnswerId { get; set; }

    public int QuestionId { get; set; }

    public string Answer { get; set; } = null!;

    public string UserId { get; set; } = null!;

    public virtual Question Question { get; set; } = null!;

    public virtual Account User { get; set; } = null!;
}
