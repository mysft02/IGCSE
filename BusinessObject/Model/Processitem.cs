using System;
using System.Collections.Generic;

namespace BusinessObject.Model;

public partial class Processitem
{
    public long ProcessItemId { get; set; }

    public long ProcessId { get; set; }

    public long LessonItemId { get; set; }

    public DateTime? CreatedAt { get; set; }

    public DateTime? UpdatedAt { get; set; }

    // Navigation properties
    public virtual Lessonitem LessonItem { get; set; } = null!;

    public virtual Process Process { get; set; } = null!;
}
