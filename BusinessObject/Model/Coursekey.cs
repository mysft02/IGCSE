using System;
using System.Collections.Generic;

namespace BusinessObject.Model;

public class Coursekey
{
    public int CourseKeyId { get; set; }

    public int CourseId { get; set; }

    public string? StudentId { get; set; }

    public string? CreatedBy { get; set; }

    public DateTime? CreatedAt { get; set; }

    public DateTime? UpdatedAt { get; set; }

    public string Status { get; set; } = "available";

    public string KeyValue { get; set; } = string.Empty;

    public virtual Course Course { get; set; } = null!;
}
