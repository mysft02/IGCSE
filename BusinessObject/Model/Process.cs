using System;
using System.Collections.Generic;

namespace BusinessObject.Model;

public partial class Process
{
    public long ProcessId { get; set; }

    public long CourseKeyId { get; set; }

    public long LessonId { get; set; }

    public DateTime? CreatedAt { get; set; }

    public DateTime? UpdatedAt { get; set; }

    // Navigation properties
    public virtual Coursekey CourseKey { get; set; } = null!;

    public virtual Lesson Lesson { get; set; } = null!;

    public virtual ICollection<Processitem> Processitems { get; set; } = new List<Processitem>();
}
