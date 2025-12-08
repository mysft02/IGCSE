namespace BusinessObject.DTOs.Request.LearningPaths
{
    public class LearningPathCreateRequest
    {
        public string Title { get; set; }

        public string Description { get; set; }

        public List<int> ModuleId { get; set; }
    }
}
