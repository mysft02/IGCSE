namespace BusinessObject.Model;

public class Processitem
{
    public int ProcessItemId { get; set; }

    public int ProcessId { get; set; }

    public int LessonItemId { get; set; }

    public DateTime? CreatedAt { get; set; }

    public DateTime? UpdatedAt { get; set; }

    public virtual Lessonitem LessonItem { get; set; } = null!;

    public virtual Process Process { get; set; } = null!;
}
