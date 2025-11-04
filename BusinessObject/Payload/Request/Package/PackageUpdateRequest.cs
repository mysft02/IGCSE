using Swashbuckle.AspNetCore.Annotations;

namespace BusinessObject.Payload.Request
{
    public class PackageUpdateRequest
    {
        [SwaggerSchema("ID package")]
        public int PackageId { get; set; }

        [SwaggerSchema("Tên package")]
        public string? Title { get; set; }

        [SwaggerSchema("Mô tả package")]
        public string? Description { get; set; }

        [SwaggerSchema("Trạng thái package (true là họat động, false là ngừng hoạt động)")]
        public bool? IsActive { get; set; }

        [SwaggerSchema("Giá package")]
        public decimal? Price { get; set; }

        [SwaggerSchema("Số lượng tạo khóa học của package (Nếu dùng cho mocktest thì mặc định là 0)")]
        public int? Slot { get; set; }

        [SwaggerSchema("Loại package dùng cho học sinh thi thử hoặc giáo viên tạo khóa học")]
        public bool? IsMockTest { get; set; }
    }
}
