using System;
namespace BusinessObject.Model;

public class Finalquizresult
{
    public int FinalQuizResultId { get; set; }

    public int FinalQuizId { get; set; }

    public decimal Score { get; set; }

    public bool IsPassed { get; set; }

    public string UserId { get; set; } = null!;

    public DateTime CreatedAt { get; set; }

    public virtual Finalquiz FinalQuiz { get; set; }
}
