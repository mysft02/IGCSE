namespace BusinessObject.DTOs.Response.Courses
{
    public class PagedResponse<T>
    {
        public int CurrentPage { get; set; }

        public int PageSize { get; set; }

        public int TotalPages { get; set; }

        public int TotalRecords { get; set; }

        public IEnumerable<T> Data { get; set; } = new List<T>();
    }
}

