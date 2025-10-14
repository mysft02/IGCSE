using System;
using System.Collections.Generic;

namespace BusinessObject.Model;

public partial class Quizresult
{
    public int QuizResultId { get; set; }

    public int QuizId { get; set; }

    public decimal Score { get; set; }

    public DateTime CreatedAt { get; set; }

    public string CreatedBy { get; set; } = null!;

    public bool? IsPassed { get; set; }
    public virtual Quiz Quiz { get; set; } = null!;
}
