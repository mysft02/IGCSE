namespace BusinessObject.DTOs.Request.Chapters
{
    public class ChapterRequest
    {
        public int ModuleID { get; set; }

        public string ChapterName { get; set; } = string.Empty;

        public string? ChapterDescription { get; set; }
    }
}
