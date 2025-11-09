namespace BusinessObject.Model;

public class Finalquizuseranswer
{
    public int FinalQuizUserAnswerId { get; set; }

    public int FinalQuizResultId { get; set; }

    public int QuestionId { get; set; }

    public string? Answer { get; set; }
}
