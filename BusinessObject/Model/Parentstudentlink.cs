using System;
using System.Collections.Generic;

namespace BusinessObject.Model;

public partial class Parentstudentlink
{
    public int LinkId { get; set; }

    public string ParentId { get; set; } = null!;

    public string StudentId { get; set; } = null!;
}
