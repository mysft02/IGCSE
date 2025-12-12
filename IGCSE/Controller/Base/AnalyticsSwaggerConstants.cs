namespace IGCSE.Controller.Base
{
    /// <summary>
    /// Constants cho Swagger documentation của Analytics APIs
    /// Sử dụng các constants này trong [SwaggerOperation] attributes
    /// </summary>
    public static class AnalyticsSwaggerConstants
    {
        // Common error scenarios
        public const string UserNotFoundError = @"**Không tìm thấy thông tin người dùng:**
```json
{
  ""message"": ""Không tìm thấy thông tin người dùng"",
  ""statusCode"": 500,
  ""data"": null
}
```";

        public const string UnauthorizedError = @"**Không có quyền truy cập:**
```json
{
  ""message"": ""You don't have permission to access this resource"",
  ""statusCode"": 403,
  ""data"": null
}
```";

        public const string NotFoundError = @"**Không tìm thấy:**
```json
{
  ""message"": ""Resource not found"",
  ""statusCode"": 400,
  ""data"": null
}
```";

        // Common notes
        public const string TeacherOnlyNote = "- Chỉ Teacher role mới có quyền sử dụng API này\n- Teacher ID được lấy tự động từ JWT token";
        
        public const string AdminOnlyNote = "- Chỉ Admin role mới có quyền sử dụng API này";
        
        public const string StudentOnlyNote = "- Chỉ Student role mới có quyền sử dụng API này\n- Student ID được lấy tự động từ JWT token";

        /// <summary>
        /// Build description từ các parts (helper method để build const string)
        /// </summary>
        public static string BuildDescription(
            string summary,
            string? requestDescription = null,
            string? responseSchemaJson = null,
            string? errorScenarios = null,
            string? notes = null)
        {
            var description = summary;

            if (!string.IsNullOrEmpty(requestDescription))
            {
                description += $"\n\n**Request:**\n{requestDescription}";
            }

            if (!string.IsNullOrEmpty(responseSchemaJson))
            {
                description += $"\n\n**Response Schema:**\n```json\n{responseSchemaJson}\n```";
            }

            if (!string.IsNullOrEmpty(errorScenarios))
            {
                description += $"\n\n**Error Scenarios:**\n\n{errorScenarios}";
            }

            if (!string.IsNullOrEmpty(notes))
            {
                description += $"\n\n**Lưu ý:**\n{notes}";
            }

            return description;
        }
    }
}

