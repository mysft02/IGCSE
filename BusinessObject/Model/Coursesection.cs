using System;
using System.Collections.Generic;

namespace BusinessObject.Model;

public partial class Coursesection
{
    public long CourseSectionId { get; set; }

    public virtual Course Course { get; set; } = null!;

    public long CourseId { get; set; }

    public string Name { get; set; } = null!;

    public string? Description { get; set; }

    public int Order { get; set; }

    public sbyte IsActive { get; set; }

    public DateTime? CreatedAt { get; set; }

    public DateTime? UpdatedAt { get; set; }
}
