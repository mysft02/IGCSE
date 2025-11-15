namespace BusinessObject.Model;

public class Process
{
    public int ProcessId { get; set; }


    public int CourseId { get; set; }

    public string StudentId { get; set; } = string.Empty;

    public int LessonId { get; set; }

    public DateTime? CreatedAt { get; set; }

    public DateTime? UpdatedAt { get; set; }

    public bool IsUnlocked { get; set; }

    // public virtual Coursekey? CourseKey { get; set; } // removed coursekey usage

    public virtual Lesson Lesson { get; set; } = null!;
}
