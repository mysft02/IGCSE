namespace DTOs.Request.Courses
{
    public class CourseListQuery
    {
        public int Page { get; set; } = 1;
        public int PageSize { get; set; } = 10;
        public string? SearchByName { get; set; } = null;
        public long? CouseId { get; set; } = null; // filter by specific course id
        public string? Status { get; set; } = null;
    }
}
