using System.ComponentModel.DataAnnotations;

namespace DTOs.Request.Categories
{
    public class CategoryRequest
    {
        [Required]
        [StringLength(200)]
        public string CategoryName { get; set; } = string.Empty;

        [StringLength(4000)]
        public string Description { get; set; } = string.Empty;

        public bool IsActive { get; set; } = true;
    }
}
