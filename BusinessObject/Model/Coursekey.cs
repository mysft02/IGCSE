using System;
using System.Collections.Generic;

namespace BusinessObject.Model;

public partial class Coursekey
{
    public long CourseKeyId { get; set; }

    public long CourseId { get; set; }

    public long StudentId { get; set; }

    public string? CreatedBy { get; set; }

    public DateTime? CreatedAt { get; set; }

    public DateTime? UpdatedAt { get; set; }

    // Navigation properties
    public virtual Course Course { get; set; } = null!;

    public virtual Account Student { get; set; } = null!;

    public virtual ICollection<Process> Processes { get; set; } = new List<Process>();
}
