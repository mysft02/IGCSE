using System;
namespace BusinessObject.DTOs.Response.Chapters
{
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
