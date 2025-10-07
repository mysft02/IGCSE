using System;
using System.Collections.Generic;

namespace BusinessObject.Model;

public partial class Course
{
    public long CourseId { get; set; }

    public string Name { get; set; } = null!;

    public string? Description { get; set; }

    public string Status { get; set; } = null!;

    public decimal? Price { get; set; }

    public string? ImageUrl { get; set; }

    public long? CategoryId { get; set; }

    public DateTime? CreatedAt { get; set; }

    public DateTime? UpdatedAt { get; set; }

    public string? CreatedBy { get; set; }

    public string? UpdatedBy { get; set; }
}
