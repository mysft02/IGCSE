using System;
using System.Collections.Generic;

namespace BusinessObject.Model;

public partial class Coursekey
{
    public int CourseKeyId { get; set; }

    public int CourseId { get; set; }

    public string StudentId { get; set; } = null!;

    public string? CreatedBy { get; set; }

    public DateTime? CreatedAt { get; set; }

    public DateTime? UpdatedAt { get; set; }

    public virtual Course Course { get; set; } = null!;
}
