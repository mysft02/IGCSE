using System;
using System.Collections.Generic;

namespace BusinessObject.Model;

public partial class Lesson
{
    public long LessonId { get; set; }

    public long CourseSectionId { get; set; }

    public string Name { get; set; } = null!;

    public string? Description { get; set; }

    public int Order { get; set; }

    public sbyte IsActive { get; set; }

    public DateTime? CreatedAt { get; set; }

    public DateTime? UpdatedAt { get; set; }

    // Navigation properties
    public virtual Coursesection CourseSection { get; set; } = null!;

    public virtual ICollection<Process> Processes { get; set; } = new List<Process>();
}
