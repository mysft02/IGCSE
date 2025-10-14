using System;
using System.Collections.Generic;

namespace BusinessObject.Model;

public class Useranswer
{
    public int UserAnswerId { get; set; }

    public int QuestionId { get; set; }

    public string Answer { get; set; } = null!;

    public int UserId { get; set; }

    public virtual Question Question { get; set; } = null!;

    public virtual Account User { get; set; } = null!;
}
