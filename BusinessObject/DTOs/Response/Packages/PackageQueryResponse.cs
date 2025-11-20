namespace BusinessObject.DTOs.Response.Packages
{
    public class PackageQueryResponse
    {
        public int PackageId { get; set; }

        public string? Title { get; set; }

        public string? Description { get; set; }

        public bool? IsActive { get; set; }

        public decimal Price { get; set; }

        public int Slot { get; set; }

        public bool? IsMockTest { get; set; }
    }
}
