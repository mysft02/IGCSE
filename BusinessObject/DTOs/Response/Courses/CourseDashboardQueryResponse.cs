namespace BusinessObject.DTOs.Response.Courses
{
    public class CourseDashboardQueryResponse
    {
        public int CourseId { get; set; }

        public string CourseName { get; set; }

        public string CourseDescription { get; set; }

        public string Status { get; set; }

        public decimal Price { get; set; }

        public string ImageUrl { get; set; }

        public DateTime CreatedAt { get; set; }

        public DateTime UpdatedAt { get; set; }

        public string CreatedBy { get; set; }

        public int CustomerCount { get; set; }

        public decimal AverageFinalScore { get; set; }

        public decimal TotalIncome { get; set; }
    }
}
