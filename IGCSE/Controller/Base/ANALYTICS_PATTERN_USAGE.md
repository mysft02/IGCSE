# Analytics API Pattern - Hướng dẫn sử dụng

## Tổng quan

Pattern này cung cấp một cách tiếp cận chung để xây dựng các Analytics APIs, giúp:
- Giảm code duplication
- Chuẩn hóa error handling
- Tự động hóa Swagger documentation
- Dễ dàng mở rộng cho các analytics mới

## Cấu trúc

### 1. BaseAnalyticsController
Base class cung cấp các methods chung:
- `GetUserId()` - Lấy userId từ JWT token
- `ValidateUser()` - Validate user và throw exception nếu không tìm thấy
- `HandleAnalyticsAsync<TResponse>()` - Generic method để xử lý analytics endpoints

### 2. AnalyticsSwaggerHelper
Helper class để tạo Swagger documentation:
- `BuildAnalyticsDescription()` - Tạo description với schema, errors, notes
- `BuildErrorScenario()` - Tạo error scenario description
- `BuildErrorScenarios()` - Tạo multiple error scenarios
- `GetUserNotFoundError()` - Standard user not found error
- `GetUnauthorizedError()` - Standard unauthorized error

## Cách sử dụng

### Ví dụ 1: Analytics không có parameter

```csharp
[HttpGet("get-teacher-analytics")]
[Authorize(Roles = "Teacher")]
[SwaggerOperation(
    Summary = "Lấy thống kê tài khoản cho giáo viên",
    Description = AnalyticsSwaggerHelper.BuildAnalyticsDescription(
        summary: "Api lấy thống kê khoá học và thu nhập của giáo viên",
        responseSchemaJson: @"{
  ""message"": ""Lấy thống kê cho giáo viên thành công."",
  ""statusCode"": 200,
  ""data"": {
    ""course"": {
      ""totalCourses"": 40,
      ""pending"": 9,
      ""open"": 21
    }
  }
}",
        errorScenarios: AnalyticsSwaggerHelper.GetUserNotFoundError(),
        notes: "- Chỉ Teacher role mới có quyền sử dụng API này"
    ))]
public async Task<ActionResult<BaseResponse<TeacherAnalyticsResponse>>> GetTotalAnalytics()
{
    return await HandleAnalyticsAsync(
        _courseService.GetTeacherAnalytics
    );
}
```

### Ví dụ 2: Analytics có parameter

```csharp
[HttpGet("get-courses-analytics")]
[Authorize(Roles = "Teacher")]
[SwaggerOperation(
    Summary = "Lấy thống kê khoá học",
    Description = AnalyticsSwaggerHelper.BuildAnalyticsDescription(
        summary: "Api xem thống kê chi tiết về một khóa học",
        requestDescription: "- Query parameter: `courseId` (int, required) - ID của khóa học",
        responseSchemaJson: @"{
  ""message"": ""Lấy thống kê khoá học thành công"",
  ""statusCode"": 200,
  ""data"": {
    ""courseId"": 34,
    ""courseName"": ""Tên khóa học"",
    ""customerCount"": 150
  }
}",
        errorScenarios: AnalyticsSwaggerHelper.BuildErrorScenarios(
            ("Không tìm thấy thông tin người dùng", "Không tìm thấy thông tin người dùng", 500),
            ("Khóa học không tồn tại", "Course not found", 400),
            ("Không có quyền truy cập", "You don't have permission", 403)
        ),
        notes: "- Chỉ Teacher role mới có quyền sử dụng API này"
    ))]
public async Task<ActionResult<BaseResponse<CourseAnalyticsResponse>>> GetCourseAnalytics([FromQuery] int courseId)
{
    return await HandleAnalyticsAsync(
        courseId,
        _courseService.GetCourseAnalyticsAsync
    );
}
```

### Ví dụ 3: Analytics với userId và parameter

```csharp
[HttpGet("get-student-progress")]
[Authorize(Roles = "Teacher")]
[SwaggerOperation(
    Summary = "Lấy tiến độ học tập của học sinh",
    Description = AnalyticsSwaggerHelper.BuildAnalyticsDescription(
        summary: "Api xem tiến độ học tập của một học sinh trong khóa học",
        requestDescription: "- Query parameter: `studentId` (string, required) - ID của học sinh\n- Query parameter: `courseId` (int, required) - ID của khóa học",
        responseSchemaJson: @"{
  ""message"": ""Lấy tiến độ học tập thành công"",
  ""statusCode"": 200,
  ""data"": {
    ""studentId"": ""user123"",
    ""courseId"": 34,
    ""progress"": 75.5,
    ""completedLessons"": 15,
    ""totalLessons"": 20
  }
}",
        errorScenarios: AnalyticsSwaggerHelper.BuildErrorScenarios(
            ("Không tìm thấy thông tin người dùng", "Không tìm thấy thông tin người dùng", 500),
            ("Học sinh không tồn tại", "Student not found", 400)
        )
    ))]
public async Task<ActionResult<BaseResponse<StudentProgressResponse>>> GetStudentProgress(
    [FromQuery] string studentId, 
    [FromQuery] int courseId)
{
    // Nếu service method cần cả userId và parameter
    var userId = ValidateUser();
    var result = await _courseService.GetStudentProgressAsync(userId, studentId, courseId);
    return Ok(result);
}
```

## Service Method Signature

Service methods nên follow pattern này:

```csharp
// Pattern 1: Không có parameter, chỉ cần userId
public async Task<BaseResponse<TeacherAnalyticsResponse>> GetTeacherAnalytics(string userId)

// Pattern 2: Có parameter, không cần userId
public async Task<BaseResponse<CourseAnalyticsResponse>> GetCourseAnalyticsAsync(int courseId)

// Pattern 3: Có parameter và userId
public async Task<BaseResponse<StudentProgressResponse>> GetStudentProgressAsync(
    string userId, 
    string studentId, 
    int courseId)
```

## Lợi ích

1. **Code Reusability**: Giảm code duplication, dễ maintain
2. **Consistency**: Tất cả analytics APIs follow cùng một pattern
3. **Error Handling**: Standardized error handling và validation
4. **Documentation**: Swagger docs được generate tự động, consistent format
5. **Extensibility**: Dễ dàng thêm analytics mới chỉ cần follow pattern

## Best Practices

1. **Luôn sử dụng `HandleAnalyticsAsync`** thay vì tự implement logic
2. **Sử dụng `AnalyticsSwaggerHelper`** để tạo Swagger docs consistent
3. **Validate user trước** khi call service methods
4. **Return proper error messages** với status codes phù hợp
5. **Document rõ ràng** request parameters và response schemas

