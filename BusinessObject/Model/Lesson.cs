namespace BusinessObject.Model;

public class Lesson
{
    public int LessonId { get; set; }

    public int CourseSectionId { get; set; }

    public string Name { get; set; } = null!;

    public string? Description { get; set; }

    public int Order { get; set; }

    public sbyte IsActive { get; set; }

    public DateTime? CreatedAt { get; set; }

    public DateTime? UpdatedAt { get; set; }

    public virtual Coursesection CourseSection { get; set; } = null!;
}
