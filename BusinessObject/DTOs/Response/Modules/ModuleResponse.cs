using System;
using System.Collections.Generic;
namespace BusinessObject.DTOs.Response.Modules
{
    public class ModuleResponse
    {
        public int ModuleID { get; set; }
        public string ModuleName { get; set; } = string.Empty;
        public string? Description { get; set; }
        public sbyte IsActive { get; set; }
        public DateTime? CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }
        public List<ChapterResponse> Chapters { get; set; } = new();
    }
    public class ChapterResponse
    {
        public int ChapterID { get; set; }
        public int ModuleID { get; set; }
        public string ChapterName { get; set; } = string.Empty;
        public string? ChapterDescription { get; set; }
        public DateTime? CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }
    }
}
