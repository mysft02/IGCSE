using System;
using System.Collections.Generic;

namespace BusinessObject.Model;

public partial class Coursekey
{
    public long CourseKeyId { get; set; }

    public virtual Course Course { get; set; } = null!;

    public long CourseId { get; set; }

    public string StudentId { get; set; } = null!;

    public string? CreatedBy { get; set; }

    public DateTime? CreatedAt { get; set; }

    public DateTime? UpdatedAt { get; set; }
}
