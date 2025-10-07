using System;
using System.Collections.Generic;

namespace BusinessObject.Model;

public partial class Course
{
    public int CourseId { get; set; }

    public string CourseName { get; set; } = null!;

    public string? Description { get; set; }

    public bool Status { get; set; }

    public decimal Price { get; set; }

    public string? ImageUrl { get; set; }

    public DateTime CreatedAt { get; set; }

    public DateTime UpdatedAt { get; set; }

    public int CategoryId { get; set; }
}
