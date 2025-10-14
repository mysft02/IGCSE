using System;
using System.Collections.Generic;

namespace BusinessObject.Model;

public class Process
{
    public int ProcessId { get; set; }

    public int CourseKeyId { get; set; }

    public int LessonId { get; set; }

    public DateTime? CreatedAt { get; set; }

    public DateTime? UpdatedAt { get; set; }

    public virtual Coursekey CourseKey { get; set; } = null!;

    public virtual Lesson Lesson { get; set; } = null!;
}
