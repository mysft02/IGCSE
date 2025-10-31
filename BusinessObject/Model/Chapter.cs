using System;
using System.Collections.Generic;

namespace BusinessObject.Model
{
    public class Chapter
    {
        public int ChapterID { get; set; }
        public int ModuleID { get; set; }
        public string ChapterName { get; set; } = null!;
        public string? ChapterDescription { get; set; }
        public DateTime? CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }
        // Navigation property
        public virtual Module Module { get; set; } = null!;
    }
}
