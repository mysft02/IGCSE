using System;
using System.Collections.Generic;

namespace BusinessObject.Model;

public partial class Question
{
    public int QuestionId { get; set; }

    public int QuizId { get; set; }

    public string QuestionContent { get; set; } = null!;

    public string CorrectAnswer { get; set; } = null!;

    public DateTime CreatedAt { get; set; }

    public DateTime UpdatedAt { get; set; }
    public virtual Quiz Quiz { get; set; } = null!;
}
