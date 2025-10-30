using System;
using System.Collections.Generic;

namespace BusinessObject.Model;

public class Mocktestquestion
{
    public int MockTestQuestionId { get; set; }

    public int MockTestId { get; set; }

    public string QuestionContent { get; set; } = null!;

    public string CorrectAnswer { get; set; } = null!;

    public string? ImageUrl { get; set; }

    public DateTime CreatedAt { get; set; }

    public DateTime UpdatedAt { get; set; }

    public virtual Mocktest MockTest { get; set; } = null!;
}
