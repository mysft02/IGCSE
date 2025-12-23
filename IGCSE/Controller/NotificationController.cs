using BusinessObject.DTOs.Request.Notifications;
using BusinessObject.DTOs.Response;
using BusinessObject.DTOs.Response.Notifications;
using Common.Constants;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Service;
using Swashbuckle.AspNetCore.Annotations;

namespace IGCSE.Controller
{
    [Route("api/notifications")]
    [ApiController]
    [Authorize]
    public class NotificationController : ControllerBase
    {
        private readonly CourseService _courseService;

        public NotificationController(CourseService courseService)
        {
            _courseService = courseService;
        }

        [HttpGet]
        [SwaggerOperation(
            Summary = "Lấy danh sách thông báo của user",
            Description = @"Api dùng để lấy danh sách thông báo của user hiện tại với phân trang và bộ lọc.

**Request:**
- Query parameters:
  - `Page` (int, mặc định: 1) - Số trang
  - `PageSize` (int, mặc định: 10) - Số lượng item mỗi trang
  - `IsRead` (bool, optional) - Lọc theo trạng thái đọc: `true` (đã đọc), `false` (chưa đọc), `null` (tất cả)
  - `Type` (string, optional) - Lọc theo loại notification: `COURSE_DISABLED`, `COURSE_APPROVED`, `COURSE_REJECTED`, ...
  - `RelatedEntityType` (string, optional) - Lọc theo loại entity liên quan: `COURSE`, `REPORT`, ...
  - `SearchByTitle` (string, optional) - Tìm kiếm theo tiêu đề notification

**Response Schema - Trường hợp thành công:**
```json
{
  ""message"": ""Lấy danh sách thông báo thành công"",
  ""statusCode"": 200,
  ""data"": {
    ""items"": [
      {
        ""notificationId"": 1,
        ""userId"": ""user-id-123"",
        ""title"": ""Khóa học đã bị ẩn"",
        ""message"": ""Lý do ẩn khóa học"",
        ""type"": ""COURSE_DISABLED"",
        ""isRead"": false,
        ""relatedEntityType"": ""COURSE"",
        ""relatedEntityId"": 34,
        ""createdAt"": ""2024-01-15T10:30:00Z"",
        ""updatedAt"": null
      }
    ],
    ""totalCount"": 25,
    ""page"": 0,
    ""size"": 10,
    ""totalPages"": 3,
    ""hasNextPage"": true,
    ""hasPreviousPage"": false,
    ""currentPage"": 1
  }
}
```

**Response Schema - Trường hợp lỗi:**
```json
{
  ""message"": ""Lỗi khi lấy danh sách thông báo: Error message"",
  ""statusCode"": 400,
  ""data"": null
}
```

**Lưu ý:**
- Cần đăng nhập (Authorize)
- User ID được lấy tự động từ JWT token
- Kết quả được sắp xếp theo thời gian tạo mới nhất (desc)
- Có thể kết hợp nhiều filter cùng lúc")]
        public async Task<ActionResult<BaseResponse<PaginatedResponse<NotificationResponse>>>> GetNotifications(
            [FromQuery] NotificationQueryRequest request)
        {
            try
            {
                var userId = User.FindFirst("AccountID")?.Value;
                if (string.IsNullOrEmpty(userId))
                {
                    return Unauthorized(new BaseResponse<string>(
                        "Không xác định được tài khoản.",
                        StatusCodeEnum.Unauthorized_401,
                        null));
                }

                var result = await _courseService.GetNotificationsAsync(userId, request);
                return Ok(result);
            }
            catch (Exception ex)
            {
                return BadRequest(new BaseResponse<string>(
                    $"Lỗi khi lấy danh sách thông báo: {ex.Message}",
                    StatusCodeEnum.BadRequest_400,
                    null
                ));
            }
        }

        [HttpGet("unread-count")]
        [SwaggerOperation(
            Summary = "Lấy số lượng thông báo chưa đọc",
            Description = @"Api dùng để lấy số lượng thông báo chưa đọc của user hiện tại. Thường được dùng để hiển thị badge số lượng thông báo chưa đọc trên UI.

**Request:**
- Không cần query parameters
- User ID được lấy tự động từ JWT token

**Response Schema - Trường hợp thành công:**
```json
{
  ""message"": ""Lấy số lượng thông báo chưa đọc thành công"",
  ""statusCode"": 200,
  ""data"": 5
}
```

**Response Schema - Trường hợp lỗi:**
```json
{
  ""message"": ""Lỗi khi lấy số lượng thông báo chưa đọc: Error message"",
  ""statusCode"": 400,
  ""data"": null
}
```

**Lưu ý:**
- Cần đăng nhập (Authorize)
- User ID được lấy tự động từ JWT token
- Dùng để hiển thị badge số lượng thông báo chưa đọc trên UI
- Kết quả trả về là số nguyên (int)")]
        public async Task<ActionResult<BaseResponse<int>>> GetUnreadCount()
        {
            try
            {
                var userId = User.FindFirst("AccountID")?.Value;
                if (string.IsNullOrEmpty(userId))
                {
                    return Unauthorized(new BaseResponse<string>(
                        "Không xác định được tài khoản.",
                        StatusCodeEnum.Unauthorized_401,
                        null));
                }

                var result = await _courseService.GetUnreadNotificationCountAsync(userId);
                return Ok(result);
            }
            catch (Exception ex)
            {
                return BadRequest(new BaseResponse<string>(
                    $"Lỗi khi lấy số lượng thông báo chưa đọc: {ex.Message}",
                    StatusCodeEnum.BadRequest_400,
                    null
                ));
            }
        }

        [HttpPut("{notificationId}/read")]
        [SwaggerOperation(
            Summary = "Đánh dấu thông báo đã đọc",
            Description = @"Api dùng để đánh dấu một thông báo là đã đọc. Sau khi đánh dấu, `isRead` sẽ được set thành `true` và `updatedAt` sẽ được cập nhật.

**Request:**
- Path parameter: `notificationId` (int, required) - ID của thông báo cần đánh dấu đã đọc
- Không cần body request

**Response Schema - Trường hợp thành công:**
```json
{
  ""message"": ""Đánh dấu thông báo đã đọc thành công"",
  ""statusCode"": 200,
  ""data"": {
    ""notificationId"": 1,
    ""userId"": ""user-id-123"",
    ""title"": ""Khóa học đã bị ẩn"",
    ""message"": ""Lý do ẩn khóa học"",
    ""type"": ""COURSE_DISABLED"",
    ""isRead"": true,
    ""relatedEntityType"": ""COURSE"",
    ""relatedEntityId"": 34,
    ""createdAt"": ""2024-01-15T10:30:00Z"",
    ""updatedAt"": ""2024-01-15T11:00:00Z""
  }
}
```

**Response Schema - Trường hợp lỗi:**

1. **Thông báo không tồn tại:**
```json
{
  ""message"": ""Lỗi khi đánh dấu thông báo đã đọc: Notification not found"",
  ""statusCode"": 400,
  ""data"": null
}
```

2. **Không có quyền đánh dấu thông báo này:**
```json
{
  ""message"": ""Lỗi khi đánh dấu thông báo đã đọc: Bạn không có quyền đánh dấu thông báo này"",
  ""statusCode"": 400,
  ""data"": null
}
```

**Lưu ý:**
- Cần đăng nhập (Authorize)
- User ID được lấy tự động từ JWT token
- Chỉ có thể đánh dấu thông báo của chính mình
- Sau khi đánh dấu, `isRead` = `true` và `updatedAt` được cập nhật
- Có thể gọi nhiều lần với cùng `notificationId` (idempotent)")]
        public async Task<ActionResult<BaseResponse<NotificationResponse>>> MarkAsRead(int notificationId)
        {
            try
            {
                var userId = User.FindFirst("AccountID")?.Value;
                if (string.IsNullOrEmpty(userId))
                {
                    return Unauthorized(new BaseResponse<string>(
                        "Không xác định được tài khoản.",
                        StatusCodeEnum.Unauthorized_401,
                        null));
                }

                var result = await _courseService.MarkNotificationAsReadAsync(notificationId, userId);
                return Ok(result);
            }
            catch (Exception ex)
            {
                return BadRequest(new BaseResponse<string>(
                    $"Lỗi khi đánh dấu thông báo đã đọc: {ex.Message}",
                    StatusCodeEnum.BadRequest_400,
                    null
                ));
            }
        }
    }
}
