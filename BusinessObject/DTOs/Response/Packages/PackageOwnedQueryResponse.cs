namespace BusinessObject.DTOs.Response.Packages
{
    public class PackageOwnedQueryResponse
    {
        public int PackageId { get; set; }

        public string Title { get; set; }

        public string Description { get; set; }

        public bool IsActive { get; set; }

        public decimal Price { get; set; }

        public int Slot { get; set; }

        public bool? IsMockTest { get; set; }

        public DateTime? BuyDate { get; set; }

        public decimal? BuyPrice { get; set; }
    }
}
