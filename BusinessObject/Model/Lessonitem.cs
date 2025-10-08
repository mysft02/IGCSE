using System;
using System.Collections.Generic;

namespace BusinessObject.Model;

public partial class Lessonitem
{
    public int LessonItemId { get; set; }

    public int LessonId { get; set; }

    public string Name { get; set; } = null!;

    public string? Description { get; set; }

    public string? Content { get; set; }

    public string ItemType { get; set; } = null!;

    public int Order { get; set; }

    public DateTime? CreatedAt { get; set; }

    public DateTime? UpdatedAt { get; set; }
}
