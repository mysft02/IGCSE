using Swashbuckle.AspNetCore.Annotations;

namespace BusinessObject.Payload.Request.VnPay
{
    public class PaymentRequest
    {
        [SwaggerSchema("Id của khóa học muốn mua", Nullable = false)]
        public int CourseId { get; set; }
    }
}
