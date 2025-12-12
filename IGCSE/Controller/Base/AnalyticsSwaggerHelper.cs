namespace IGCSE.Controller.Base
{
    /// <summary>
    /// Helper class để tạo Swagger documentation cho Analytics APIs
    /// </summary>
    public static class AnalyticsSwaggerHelper
    {
        /// <summary>
        /// Tạo Swagger description cho analytics endpoint với response schema
        /// Sử dụng method này khi cần dynamic description (không dùng trong attributes)
        /// </summary>
        public static string BuildAnalyticsDescription(
            string summary,
            string? requestDescription = null,
            string? responseSchemaJson = null,
            string? errorScenarios = null,
            string? notes = null)
        {
            var description = $@"{summary}";

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
                description += $"\n\n**Error Scenarios:**\n{errorScenarios}";
            }

            if (!string.IsNullOrEmpty(notes))
            {
                description += $"\n\n**Lưu ý:**\n{notes}";
            }

            return description;
        }

        /// <summary>
        /// Build description từ các parts (dùng trong attributes với const strings)
        /// </summary>
        public static string BuildDescription(
            string summary,
            string? requestDescription = null,
            string? responseSchemaJson = null,
            string? errorScenarios = null,
            string? notes = null)
        {
            return BuildAnalyticsDescription(summary, requestDescription, responseSchemaJson, errorScenarios, notes);
        }

        /// <summary>
        /// Tạo error scenario description
        /// </summary>
        public static string BuildErrorScenario(string title, string message, int statusCode)
        {
            return $@"**{title}:**
```json
{{
  ""message"": ""{message}"",
  ""statusCode"": {statusCode},
  ""data"": null
}}
```";
        }

        /// <summary>
        /// Tạo multiple error scenarios
        /// </summary>
        public static string BuildErrorScenarios(params (string title, string message, int statusCode)[] scenarios)
        {
            return string.Join("\n\n", scenarios.Select(s => BuildErrorScenario(s.title, s.message, s.statusCode)));
        }

        /// <summary>
        /// Tạo standard user not found error
        /// </summary>
        public static string GetUserNotFoundError()
        {
            return BuildErrorScenario(
                "Không tìm thấy thông tin người dùng",
                "Không tìm thấy thông tin người dùng",
                500
            );
        }

        /// <summary>
        /// Tạo standard unauthorized error
        /// </summary>
        public static string GetUnauthorizedError()
        {
            return BuildErrorScenario(
                "Không có quyền truy cập",
                "You don't have permission to access this resource",
                403
            );
        }
    }
}

