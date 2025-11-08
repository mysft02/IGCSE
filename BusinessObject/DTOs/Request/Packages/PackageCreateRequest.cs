using Swashbuckle.AspNetCore.Annotations;

namespace BusinessObject.DTOs.Request.Packages
{
    public class PackageCreateRequest
    {
        [SwaggerSchema("Tên package")]
        public string Title { get; set; }

        [SwaggerSchema("Mô tả package")]
        public string Description { get; set; }

        [SwaggerSchema("Giá package")]
        public decimal Price { get; set; }

        [SwaggerSchema("Số lượng tạo khóa học của package (Nếu dùng cho mocktest thì mặc định là 0)")]
        public int Slot { get; set; }

        [SwaggerSchema("Loại package dùng cho học sinh thi thử hoặc giáo viên tạo khóa học")]
        public bool IsMockTest { get; set; }
    }
}
