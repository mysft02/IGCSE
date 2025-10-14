using System;
using System.Collections.Generic;

namespace BusinessObject.Model;

public class Coursesection
{
    public int CourseSectionId { get; set; }

    public int CourseId { get; set; }

    public string Name { get; set; } = null!;

    public string? Description { get; set; }

    public int Order { get; set; }

    public sbyte IsActive { get; set; }

    public DateTime? CreatedAt { get; set; }

    public DateTime? UpdatedAt { get; set; }

    public virtual Course Course { get; set; } = null!;
}
