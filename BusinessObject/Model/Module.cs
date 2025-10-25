using System;
using System.Collections.Generic;

namespace BusinessObject.Model
{
    public class Module
    {
        public int ModuleID { get; set; }
        public string ModuleName { get; set; } = null!;
        public string? Description { get; set; }
        public sbyte IsActive { get; set; }
        public DateTime? CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }
        public int CourseId { get; set; } // ForeignKey tới Course
        // Navigation property: 1 module nhiều chapter
        public virtual ICollection<Chapter> Chapters { get; set; } = new List<Chapter>();
    }
}
