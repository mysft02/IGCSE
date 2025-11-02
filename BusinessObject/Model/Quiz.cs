using System;
using System.Collections.Generic;

namespace BusinessObject.Model;

public class Quiz
{
    public int QuizId { get; set; }

    public int CourseId { get; set; }
    
    public int LessonId { get; set; }

    public string QuizTitle { get; set; } = null!;

    public string QuizDescription { get; set; } = null!;

    public DateTime? CreatedAt { get; set; }

    public DateTime? UpdatedAt { get; set; }

    public virtual Course Course { get; set; } = null!;

    public virtual ICollection<Question> Questions { get; set; } = new List<Question>();
}
