using Repository.IRepositories;
using BusinessObject.Payload.Request.Filter;
using BusinessObject.DTOs.Response.Dashboards;
using BusinessObject.DTOs.Response.Assistant;
using System.Linq;
using BusinessObject.Model;

namespace Service.Assistant;

public class SystemDataService : ISystemDataService
{
    private readonly ICourseRepository _courseRepository;
    private readonly IAccountRepository _accountRepository;
    private readonly IIntentAnalyzer _intentAnalyzer;
    private readonly IParentStudentLinkRepository _parentStudentLinkRepository;

    public SystemDataService(
        ICourseRepository courseRepository,
        IAccountRepository accountRepository,
        IIntentAnalyzer intentAnalyzer,
        IParentStudentLinkRepository parentStudentLinkRepository)
    {
        _courseRepository = courseRepository;
        _accountRepository = accountRepository;
        _intentAnalyzer = intentAnalyzer;
        _parentStudentLinkRepository = parentStudentLinkRepository;
    }

    public async Task<string> GetSystemDataContextAsync(string userId, string userRole, string userQuery)
    {
        var contextParts = new List<string>();

        Console.WriteLine($"[SystemDataService] Processing query for role {userRole}: {userQuery}");

        try
        {
            // Phân tích intent thông minh
            var intent = await _intentAnalyzer.AnalyzeIntentAsync(userQuery, userRole);
            
            Console.WriteLine($"[SystemDataService] Intent analyzed - NeedsSystemData: {intent.NeedsSystemData}, RelevanceScore: {intent.RelevanceScore}, DataTypes: [{string.Join(", ", intent.DataTypes)}]");

            if (!intent.NeedsSystemData || intent.RelevanceScore < 0.3m)
            {
                Console.WriteLine("[SystemDataService] Query does not need system data or relevance too low");
                return string.Empty;
            }

            // Lấy dữ liệu dựa trên intent
            switch (userRole)
            {
                case "Teacher":
                    await GetTeacherDataByIntentAsync(userId, intent, contextParts);
                    break;
                case "Student":
                    await GetStudentDataByIntentAsync(userId, intent, contextParts);
                    break;
                case "Parent":
                    await GetParentDataByIntentAsync(userId, intent, contextParts, userQuery);
                    break;
                case "Manager":
                    await GetManagerDataByIntentAsync(userId, intent, contextParts);
                    break;
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error in SystemDataService.GetSystemDataContextAsync: {ex.Message}");
            Console.WriteLine($"Stack trace: {ex.StackTrace}");
            // Return empty context if error - don't block the flow
        }

        var result = contextParts.Any() ? string.Join("\n", contextParts) : string.Empty;
        Console.WriteLine($"[SystemDataService] Generated context length: {result.Length} characters");
        if (!string.IsNullOrEmpty(result))
        {
            Console.WriteLine($"[SystemDataService] Context preview: {result.Substring(0, Math.Min(200, result.Length))}...");
        }

        return result;
    }

    public async Task<List<SystemDataResponse>> GetStructuredSystemDataAsync(string userId, string userRole, string userQuery)
    {
        var structuredData = new List<SystemDataResponse>();

        Console.WriteLine($"[SystemDataService] Getting structured data for role {userRole}: {userQuery}");

        try
        {
            // Phân tích intent
            var intent = await _intentAnalyzer.AnalyzeIntentAsync(userQuery, userRole);

            if (!intent.NeedsSystemData || intent.RelevanceScore < 0.3m)
            {
                Console.WriteLine("[SystemDataService] Query does not need system data or relevance too low");
                return structuredData;
            }

            // Lấy structured data dựa trên intent
            switch (userRole)
            {
                case "Teacher":
                    await GetTeacherStructuredDataAsync(userId, intent, structuredData);
                    break;
                case "Student":
                    await GetStudentStructuredDataAsync(userId, intent, structuredData);
                    break;
                case "Parent":
                    await GetParentStructuredDataAsync(userId, intent, structuredData, userQuery);
                    break;
                case "Manager":
                    await GetManagerStructuredDataAsync(userId, intent, structuredData);
                    break;
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error in SystemDataService.GetStructuredSystemDataAsync: {ex.Message}");
            Console.WriteLine($"Stack trace: {ex.StackTrace}");
        }

        Console.WriteLine($"[SystemDataService] Generated {structuredData.Count} structured data items");
        return structuredData;
    }

    private async Task GetTeacherDataByIntentAsync(string userId, QueryIntent intent, List<string> contextParts)
    {
        // Lấy analytics nếu cần
        if (intent.DataTypes.Contains("analytics"))
        {
            await AddTeacherAnalyticsAsync(userId, contextParts);
        }

        // Lấy top courses nếu cần
        if (intent.DataTypes.Contains("top_courses"))
        {
            await AddTopCoursesAsync(userId, contextParts);
        }

        // Lấy top students nếu cần
        if (intent.DataTypes.Contains("top_students"))
        {
            await AddTopStudentsAsync(userId, contextParts);
        }
    }

    private async Task AddTeacherAnalyticsAsync(string userId, List<string> contextParts)
    {
        var analytics = await _courseRepository.GetAnalytics(userId);
        
        contextParts.Add("=== THỐNG KÊ KHÓA HỌC CỦA BẠN ===");
        contextParts.Add($"- Tổng số khóa học: {analytics.Metadata.TotalCourses}");
        contextParts.Add($"- Khóa học đang mở: {analytics.Metadata.Open}");
        contextParts.Add($"- Khóa học đang chờ duyệt: {analytics.Metadata.Pending}");
        contextParts.Add($"- Khóa học bị từ chối: {analytics.Metadata.Rejected}");
        
        if (analytics.Chart.Any())
        {
            var totalIncome = analytics.Chart.Sum(c => c.TotalIncome);
            var receivedIncome = analytics.Chart.Sum(c => c.ReceivedIncome);
            contextParts.Add($"- Tổng doanh thu: {totalIncome:N0} VNĐ");
            contextParts.Add($"- Đã nhận: {receivedIncome:N0} VNĐ");
        }
    }

    private async Task AddTopCoursesAsync(string userId, List<string> contextParts)
    {
        var topAnalytics = await _courseRepository.GetTopAnalyticsAsync(userId, limit: 5);
        
        if (topAnalytics.TopCourses.Any())
        {
            contextParts.Add("\n=== TOP KHÓA HỌC ĐƯỢC HỌC NHIỀU NHẤT ===");
            int rank = 1;
            foreach (var course in topAnalytics.TopCourses)
            {
                contextParts.Add($"{rank}. {course.CourseName}: {course.TotalEnrollments} học viên đăng ký, {course.ActiveStudents} học viên đang học, {course.CompletedStudents} học viên đã hoàn thành");
                rank++;
            }
        }
    }

    private async Task AddTopStudentsAsync(string userId, List<string> contextParts)
    {
        var topAnalytics = await _courseRepository.GetTopAnalyticsAsync(userId, limit: 5);
        
        if (topAnalytics.TopStudents.Any())
        {
            contextParts.Add("\n=== TOP HỌC VIÊN TIỀM NĂNG NHẤT ===");
            int rank = 1;
            foreach (var student in topAnalytics.TopStudents)
            {
                var studentName = student.StudentName ?? "N/A";
                var avgScore = student.AverageScore?.ToString("F1") ?? "N/A";
                contextParts.Add($"{rank}. {studentName}: Điểm TB {avgScore}, {student.CompletedCourses} khóa học");
                rank++;
            }
        }
    }

    // Deprecated: Giữ lại để backward compatibility, nhưng sẽ không dùng nữa
    [Obsolete("Use GetTeacherDataByIntentAsync instead")]
    private async Task GetTeacherDataAsync(string userId, string queryLower, List<string> contextParts)
    {
        // Kiểm tra nếu hỏi về khóa học, thống kê, analytics
        // Mở rộng từ khóa để bắt được nhiều cách hỏi hơn
        if (queryLower.Contains("khóa học") || queryLower.Contains("course") ||
            queryLower.Contains("khoá học") || queryLower.Contains("khoa hoc") ||
            queryLower.Contains("ổn định") || queryLower.Contains("thống kê") ||
            queryLower.Contains("analytics") || queryLower.Contains("học viên") ||
            queryLower.Contains("doanh thu") || queryLower.Contains("revenue") ||
            queryLower.Contains("enrollment") || queryLower.Contains("đăng ký") ||
            queryLower.Contains("tốt nhất") || queryLower.Contains("hay nhất") ||
            queryLower.Contains("best") || queryLower.Contains("của tôi") ||
            queryLower.Contains("của bạn") || queryLower.Contains("của mình"))
        {
            // Lấy analytics của teacher
            var analytics = await _courseRepository.GetAnalytics(userId);
            
            contextParts.Add("=== THỐNG KÊ KHÓA HỌC CỦA BẠN ===");
            contextParts.Add($"- Tổng số khóa học: {analytics.Metadata.TotalCourses}");
            contextParts.Add($"- Khóa học đang mở: {analytics.Metadata.Open}");
            contextParts.Add($"- Khóa học đang chờ duyệt: {analytics.Metadata.Pending}");
            contextParts.Add($"- Khóa học bị từ chối: {analytics.Metadata.Rejected}");
            
            // Tính tổng income từ chart data nếu có
            if (analytics.Chart.Any())
            {
                var totalIncome = analytics.Chart.Sum(c => c.TotalIncome);
                var receivedIncome = analytics.Chart.Sum(c => c.ReceivedIncome);
                contextParts.Add($"- Tổng doanh thu: {totalIncome:N0} VNĐ");
                contextParts.Add($"- Đã nhận: {receivedIncome:N0} VNĐ");
            }
            
            // Lấy top analytics (courses và students) nếu hỏi về top/phổ biến/ổn định/tốt nhất
            if (queryLower.Contains("phổ biến") || queryLower.Contains("nhiều") || 
                queryLower.Contains("top") || queryLower.Contains("ổn định") ||
                queryLower.Contains("tốt nhất") || queryLower.Contains("hay nhất") ||
                queryLower.Contains("best") || queryLower.Contains("xuất sắc") ||
                queryLower.Contains("học viên") || queryLower.Contains("student") ||
                queryLower.Contains("tiềm năng") || queryLower.Contains("giỏi") ||
                queryLower.Contains("nổi bật") || queryLower.Contains("được yêu thích"))
            {
                var topAnalytics = await _courseRepository.GetTopAnalyticsAsync(userId, limit: 5);
                
                if (topAnalytics.TopCourses.Any())
                {
                    contextParts.Add("\n=== TOP KHÓA HỌC ĐƯỢC HỌC NHIỀU NHẤT ===");
                    int rank = 1;
                    foreach (var course in topAnalytics.TopCourses)
                    {
                        contextParts.Add($"{rank}. {course.CourseName}: {course.TotalEnrollments} học viên đăng ký, {course.ActiveStudents} học viên đang học, {course.CompletedStudents} học viên đã hoàn thành");
                        rank++;
                    }
                }

                if (topAnalytics.TopStudents.Any())
                {
                    contextParts.Add("\n=== TOP HỌC VIÊN TIỀM NĂNG NHẤT ===");
                    int rank = 1;
                    foreach (var student in topAnalytics.TopStudents)
                    {
                        var studentName = student.StudentName ?? "N/A";
                        var avgScore = student.AverageScore?.ToString("F1") ?? "N/A";
                        contextParts.Add($"{rank}. {studentName}: Điểm TB {avgScore}, {student.CompletedCourses} khóa học");
                        rank++;
                    }
                }
            }
        }
    }

    private async Task GetStudentDataByIntentAsync(string userId, QueryIntent intent, List<string> contextParts)
    {
        if (intent.DataTypes.Contains("progress"))
        {
            await AddStudentProgressAsync(userId, contextParts);
        }

        if (intent.DataTypes.Contains("statistics"))
        {
            await AddStudentStatisticsAsync(userId, contextParts);
        }

        if (intent.DataTypes.Contains("recommendations"))
        {
            await AddStudentRecommendationsAsync(userId, contextParts);
        }
    }

    private async Task AddStudentProgressAsync(string userId, List<string> contextParts)
    {
        var progress = await _courseRepository.GetStudentLearningProgressAsync(userId);
        
        contextParts.Add("=== TIẾN ĐỘ HỌC TẬP CỦA BẠN ===");
        contextParts.Add($"- Số giờ học: {progress.Metadata.TotalStudyHours:F1} giờ");
        contextParts.Add($"- Số bài học đã hoàn thành: {progress.Metadata.TotalCompletedLessonItems}");
        contextParts.Add($"- Số khóa học đang học: {progress.Metadata.ActiveCourses}");
    }

    private async Task AddStudentStatisticsAsync(string userId, List<string> contextParts)
    {
        var courseStats = await _courseRepository.GetStudentCourseStatisticsAsync(userId);
        
        contextParts.Add("\n=== THỐNG KÊ KHÓA HỌC ===");
        contextParts.Add($"- Khóa học đang học: {courseStats.Metadata.InProgressCourses}");
        contextParts.Add($"- Khóa học đã hoàn thành: {courseStats.Metadata.CompletedCourses}");
        contextParts.Add($"- Tổng số khóa học đã đăng ký: {courseStats.Metadata.TotalEnrolledCourses}");
    }

    private async Task AddStudentRecommendationsAsync(string userId, List<string> contextParts)
    {
        var recommendationsResponse = await _courseRepository.GetCourseRecommendationsAsync(userId, limit: 5);
        if (recommendationsResponse.RecommendedCourses.Any())
        {
            contextParts.Add("\n=== KHÓA HỌC GỢI Ý CHO BẠN ===");
            int rank = 1;
            foreach (var rec in recommendationsResponse.RecommendedCourses)
            {
                contextParts.Add($"{rank}. {rec.CourseName}: {rec.RecommendationReason}");
                rank++;
            }
        }
    }

    private async Task GetParentDataByIntentAsync(string parentId, QueryIntent intent, List<string> contextParts, string? userQuery = null)
    {
        if (intent.DataTypes.Contains("progress"))
        {
            await AddParentChildProgressAsync(parentId, contextParts);
        }

        if (intent.DataTypes.Contains("statistics"))
        {
            await AddParentChildStatisticsAsync(parentId, contextParts);
        }

        if (intent.DataTypes.Contains("performance"))
        {
            await AddParentChildPerformanceAsync(parentId, contextParts);
        }

        if (intent.DataTypes.Contains("certificates"))
        {
            await AddParentChildCertificatesAsync(parentId, contextParts);
        }

        if (intent.DataTypes.Contains("recommendations"))
        {
            await AddParentChildRecommendationsAsync(parentId, contextParts, userQuery);
        }
    }

    private async Task AddParentChildProgressAsync(string parentId, List<string> contextParts)
    {
        var progress = await _courseRepository.GetParentChildLearningProgressAsync(parentId);
        
        contextParts.Add("=== TIẾN ĐỘ HỌC TẬP CỦA CON ===");
        contextParts.Add($"- Số giờ học: {progress.Metadata.TotalStudyHours:F1} giờ");
        contextParts.Add($"- Số bài học đã hoàn thành: {progress.Metadata.TotalCompletedLessonItems}");
        contextParts.Add($"- Tổng số con: {progress.Metadata.TotalChildren}");
    }

    private async Task AddParentChildStatisticsAsync(string parentId, List<string> contextParts)
    {
        var courseStats = await _courseRepository.GetParentChildCourseStatisticsAsync(parentId);
        
        contextParts.Add("\n=== THỐNG KÊ KHÓA HỌC CỦA CON ===");
        contextParts.Add($"- Khóa học đang học: {courseStats.Metadata.InProgressCourses}");
        contextParts.Add($"- Khóa học đã hoàn thành: {courseStats.Metadata.CompletedCourses}");
        contextParts.Add($"- Tổng số khóa học đã đăng ký: {courseStats.Metadata.TotalEnrolledCourses}");
    }

    private async Task AddParentChildPerformanceAsync(string parentId, List<string> contextParts)
    {
        var performance = await _courseRepository.GetParentChildPerformanceAsync(parentId);
        
        contextParts.Add("\n=== THÀNH TÍCH HỌC TẬP CỦA CON ===");
        contextParts.Add($"- Điểm trung bình: {performance.Metadata.AverageScore:F1}/10");
        contextParts.Add($"- Số bài quiz đã làm: {performance.Metadata.TotalQuizzesTaken}");
        contextParts.Add($"- Tỷ lệ pass quiz: {performance.Metadata.QuizPassRate:F1}%");
    }

    private async Task AddParentChildCertificatesAsync(string parentId, List<string> contextParts)
    {
        var certificates = await _courseRepository.GetParentChildCertificatesAsync(parentId);
        
        if (certificates.Certificates.Any())
        {
            contextParts.Add("\n=== CHỨNG CHỈ CỦA CON ===");
            foreach (var cert in certificates.Certificates)
            {
                contextParts.Add($"- {cert.CourseName}: Đạt được vào {cert.CreatedAt:dd/MM/yyyy}");
            }
        }
    }

    private async Task AddParentChildRecommendationsAsync(string parentId, List<string> contextParts, string? userQuery = null)
    {
        // Lấy danh sách con của parent
        var children = await _parentStudentLinkRepository.GetByParentId(parentId);
        var childrenList = children.ToList();
        
        if (!childrenList.Any())
        {
            contextParts.Add("\n=== KHÓA HỌC GỢI Ý ===");
            contextParts.Add("Bạn chưa có con được liên kết trong hệ thống.");
            return;
        }

        // Nếu có tên học sinh trong câu hỏi, tìm đúng học sinh đó (fuzzy matching)
        var targetChildren = childrenList;
        if (!string.IsNullOrEmpty(userQuery))
        {
            var queryLower = userQuery.ToLowerInvariant();
            // Loại bỏ dấu và khoảng trắng để so sánh tốt hơn
            var normalizedQuery = NormalizeVietnameseName(queryLower);
            
            var matchedChild = childrenList.FirstOrDefault(c => 
            {
                if (string.IsNullOrEmpty(c.UserName)) return false;
                
                var childNameLower = c.UserName.ToLowerInvariant();
                var normalizedChildName = NormalizeVietnameseName(childNameLower);
                
                // Kiểm tra: query chứa tên hoặc tên chứa query
                return queryLower.Contains(childNameLower) || 
                       childNameLower.Contains(queryLower) ||
                       normalizedQuery.Contains(normalizedChildName) ||
                       normalizedChildName.Contains(normalizedQuery);
            });
            
            if (matchedChild != null)
            {
                targetChildren = new List<Account> { matchedChild };
            }
        }

        // Lấy recommendations cho từng con (hoặc con được chỉ định)
        foreach (var child in targetChildren)
        {
            var studentName = child.UserName ?? "Con của bạn";
            var recommendationsResponse = await _courseRepository.GetCourseRecommendationsAsync(child.Id, limit: 5);
            
            if (recommendationsResponse.RecommendedCourses.Any())
            {
                contextParts.Add($"\n=== KHÓA HỌC GỢI Ý CHO {studentName.ToUpper()} ===");
                int rank = 1;
                foreach (var rec in recommendationsResponse.RecommendedCourses)
                {
                    contextParts.Add($"{rank}. {rec.CourseName}: {rec.RecommendationReason}");
                    rank++;
                }
            }
            else
            {
                contextParts.Add($"\n=== KHÓA HỌC GỢI Ý CHO {studentName.ToUpper()} ===");
                contextParts.Add("Hiện tại chưa có khóa học gợi ý phù hợp.");
            }
        }
    }

    private async Task GetManagerDataByIntentAsync(string userId, QueryIntent intent, List<string> contextParts)
    {
        if (intent.DataTypes.Contains("overview"))
        {
            await AddManagerOverviewAsync(contextParts);
        }

        if (intent.DataTypes.Contains("user_statistics"))
        {
            await AddManagerUserStatisticsAsync(contextParts);
        }

        if (intent.DataTypes.Contains("statistics") && !intent.DataTypes.Contains("user_statistics") && !intent.DataTypes.Contains("revenue_statistics") && !intent.DataTypes.Contains("enrollment_statistics"))
        {
            await AddManagerCourseStatisticsAsync(contextParts);
        }

        if (intent.DataTypes.Contains("revenue_statistics"))
        {
            await AddManagerRevenueStatisticsAsync(contextParts);
        }

        if (intent.DataTypes.Contains("enrollment_statistics"))
        {
            await AddManagerEnrollmentStatisticsAsync(contextParts);
        }
    }

    private async Task AddManagerOverviewAsync(List<string> contextParts)
    {
        var overview = await _courseRepository.GetAdminOverviewAsync();
        
        contextParts.Add("=== TỔNG QUAN HỆ THỐNG ===");
        contextParts.Add($"- Tổng số người dùng: {overview.Accounts.Total}");
        contextParts.Add($"- Tổng số khóa học: {overview.Courses.Total}");
        contextParts.Add($"- Tổng số đăng ký: {overview.Enrollments.Total}");
        contextParts.Add($"- Tổng doanh thu: {overview.Revenue.Total:N0} VNĐ");
    }

    private async Task AddManagerUserStatisticsAsync(List<string> contextParts)
    {
        var userStats = await _courseRepository.GetAdminUserStatisticsAsync();
        
        contextParts.Add("\n=== THỐNG KÊ NGƯỜI DÙNG ===");
        contextParts.Add($"- Tổng số học viên: {userStats.Metadata.TotalStudents}");
        contextParts.Add($"- Tổng số giáo viên: {userStats.Metadata.TotalTeachers}");
        contextParts.Add($"- Tổng số phụ huynh: {userStats.Metadata.TotalParents}");
        contextParts.Add($"- Tổng số tài khoản: {userStats.Metadata.TotalAccounts}");
    }

    private async Task AddManagerCourseStatisticsAsync(List<string> contextParts)
    {
        var courseStats = await _courseRepository.GetAdminCourseStatisticsAsync();
        
        contextParts.Add("\n=== THỐNG KÊ KHÓA HỌC ===");
        contextParts.Add($"- Tổng số khóa học: {courseStats.Metadata.TotalCourses}");
        contextParts.Add($"- Khóa học đang mở: {courseStats.Metadata.OpenCourses}");
        contextParts.Add($"- Khóa học đang chờ duyệt: {courseStats.Metadata.PendingCourses}");
        contextParts.Add($"- Khóa học mới: {courseStats.Metadata.NewCourses}");
    }

    private async Task AddManagerRevenueStatisticsAsync(List<string> contextParts)
    {
        var revenueStats = await _courseRepository.GetAdminRevenueStatisticsAsync();
        
        contextParts.Add("\n=== THỐNG KÊ DOANH THU ===");
        contextParts.Add($"- Tổng doanh thu: {revenueStats.Metadata.TotalRevenue:N0} VNĐ");
        contextParts.Add($"- Tổng số giao dịch: {revenueStats.Metadata.TotalTransactions}");
        contextParts.Add($"- Doanh thu trung bình mỗi giao dịch: {revenueStats.Metadata.AverageTransactionAmount:N0} VNĐ");
    }

    private async Task AddManagerEnrollmentStatisticsAsync(List<string> contextParts)
    {
        var enrollmentStats = await _courseRepository.GetAdminEnrollmentStatisticsAsync();
        
        contextParts.Add("\n=== THỐNG KÊ ĐĂNG KÝ ===");
        contextParts.Add($"- Tổng số đăng ký: {enrollmentStats.Metadata.TotalEnrollments}");
        contextParts.Add($"- Học viên đang học: {enrollmentStats.Metadata.ActiveStudents}");
        contextParts.Add($"- Học viên đã hoàn thành: {enrollmentStats.Metadata.CompletedStudents}");
    }

    /// <summary>
    /// Chuẩn hóa tên tiếng Việt để so sánh (loại bỏ dấu, khoảng trắng)
    /// </summary>
    private string NormalizeVietnameseName(string name)
    {
        if (string.IsNullOrEmpty(name)) return string.Empty;
        
        // Loại bỏ dấu tiếng Việt
        var normalized = name.ToLowerInvariant()
            .Replace("à", "a").Replace("á", "a").Replace("ạ", "a").Replace("ả", "a").Replace("ã", "a")
            .Replace("ă", "a").Replace("ằ", "a").Replace("ắ", "a").Replace("ặ", "a").Replace("ẳ", "a").Replace("ẵ", "a")
            .Replace("â", "a").Replace("ầ", "a").Replace("ấ", "a").Replace("ậ", "a").Replace("ẩ", "a").Replace("ẫ", "a")
            .Replace("è", "e").Replace("é", "e").Replace("ẹ", "e").Replace("ẻ", "e").Replace("ẽ", "e")
            .Replace("ê", "e").Replace("ề", "e").Replace("ế", "e").Replace("ệ", "e").Replace("ể", "e").Replace("ễ", "e")
            .Replace("ì", "i").Replace("í", "i").Replace("ị", "i").Replace("ỉ", "i").Replace("ĩ", "i")
            .Replace("ò", "o").Replace("ó", "o").Replace("ọ", "o").Replace("ỏ", "o").Replace("õ", "o")
            .Replace("ô", "o").Replace("ồ", "o").Replace("ố", "o").Replace("ộ", "o").Replace("ổ", "o").Replace("ỗ", "o")
            .Replace("ơ", "o").Replace("ờ", "o").Replace("ớ", "o").Replace("ợ", "o").Replace("ở", "o").Replace("ỡ", "o")
            .Replace("ù", "u").Replace("ú", "u").Replace("ụ", "u").Replace("ủ", "u").Replace("ũ", "u")
            .Replace("ư", "u").Replace("ừ", "u").Replace("ứ", "u").Replace("ự", "u").Replace("ử", "u").Replace("ữ", "u")
            .Replace("ỳ", "y").Replace("ý", "y").Replace("ỵ", "y").Replace("ỷ", "y").Replace("ỹ", "y")
            .Replace("đ", "d");
        
        // Loại bỏ khoảng trắng
        return normalized.Replace(" ", "").Replace("\t", "");
    }

    #region Structured Data Methods

    private async Task GetTeacherStructuredDataAsync(string userId, QueryIntent intent, List<SystemDataResponse> structuredData)
    {
        if (intent.DataTypes.Contains("analytics"))
        {
            var analytics = await _courseRepository.GetAnalytics(userId);
            var totalIncome = analytics.Chart.Any() ? analytics.Chart.Sum(c => c.TotalIncome) : 0m;
            var receivedIncome = analytics.Chart.Any() ? analytics.Chart.Sum(c => c.ReceivedIncome) : 0m;

            structuredData.Add(new SystemDataResponse
            {
                DataType = "analytics",
                Title = "Thống kê khóa học của bạn",
                Data = new
                {
                    TotalCourses = analytics.Metadata.TotalCourses,
                    OpenCourses = analytics.Metadata.Open,
                    PendingCourses = analytics.Metadata.Pending,
                    RejectedCourses = analytics.Metadata.Rejected,
                    TotalIncome = totalIncome,
                    ReceivedIncome = receivedIncome
                }
            });
        }

        if (intent.DataTypes.Contains("top_courses") || intent.DataTypes.Contains("top_students"))
        {
            var topAnalytics = await _courseRepository.GetTopAnalyticsAsync(userId, limit: 5);
            
            if (topAnalytics.TopCourses.Any())
            {
                structuredData.Add(new SystemDataResponse
                {
                    DataType = "top_courses",
                    Title = "Top khóa học được học nhiều nhất",
                    Data = topAnalytics.TopCourses
                });
            }

            if (topAnalytics.TopStudents.Any())
            {
                structuredData.Add(new SystemDataResponse
                {
                    DataType = "top_students",
                    Title = "Top học viên tiềm năng nhất",
                    Data = topAnalytics.TopStudents
                });
            }
        }
    }

    private async Task GetStudentStructuredDataAsync(string userId, QueryIntent intent, List<SystemDataResponse> structuredData)
    {
        if (intent.DataTypes.Contains("progress"))
        {
            var progress = await _courseRepository.GetStudentLearningProgressAsync(userId);
            structuredData.Add(new SystemDataResponse
            {
                DataType = "progress",
                Title = "Tiến độ học tập của bạn",
                Data = new
                {
                    TotalStudyHours = progress.Metadata.TotalStudyHours,
                    TotalCompletedLessonItems = progress.Metadata.TotalCompletedLessonItems,
                    ActiveCourses = progress.Metadata.ActiveCourses
                }
            });
        }

        if (intent.DataTypes.Contains("statistics"))
        {
            var courseStats = await _courseRepository.GetStudentCourseStatisticsAsync(userId);
            structuredData.Add(new SystemDataResponse
            {
                DataType = "statistics",
                Title = "Thống kê khóa học",
                Data = new
                {
                    InProgressCourses = courseStats.Metadata.InProgressCourses,
                    CompletedCourses = courseStats.Metadata.CompletedCourses,
                    TotalEnrolledCourses = courseStats.Metadata.TotalEnrolledCourses
                }
            });
        }

        if (intent.DataTypes.Contains("recommendations"))
        {
            var recommendations = await _courseRepository.GetCourseRecommendationsAsync(userId, limit: 5);
            if (recommendations.RecommendedCourses.Any())
            {
                structuredData.Add(new SystemDataResponse
                {
                    DataType = "recommendations",
                    Title = "Khóa học gợi ý cho bạn",
                    Data = recommendations.RecommendedCourses
                });
            }
        }
    }

    private async Task GetParentStructuredDataAsync(string parentId, QueryIntent intent, List<SystemDataResponse> structuredData, string? userQuery = null)
    {
        var children = await _parentStudentLinkRepository.GetByParentId(parentId);
        var childrenList = children.ToList();

        if (!childrenList.Any()) return;

        // Tìm con được chỉ định (nếu có tên trong query)
        List<Account> targetChildren;
        if (!string.IsNullOrEmpty(userQuery))
        {
            var queryLower = userQuery.ToLowerInvariant();
            var normalizedQuery = NormalizeVietnameseName(queryLower);
            
            // Tìm con match với tên trong query
            var matchedChild = childrenList.FirstOrDefault(c => 
            {
                if (string.IsNullOrEmpty(c.UserName)) return false;
                var childNameLower = c.UserName.ToLowerInvariant();
                var normalizedChildName = NormalizeVietnameseName(childNameLower);
                
                // Loại bỏ khoảng trắng và số để so sánh tốt hơn
                var queryNoSpace = System.Text.RegularExpressions.Regex.Replace(normalizedQuery, @"[\d\s]+", "");
                var childNameNoSpace = System.Text.RegularExpressions.Regex.Replace(normalizedChildName, @"[\d\s]+", "");
                
                // Extract tên chính (loại bỏ số và ký tự đặc biệt)
                var queryNameOnly = System.Text.RegularExpressions.Regex.Replace(normalizedQuery, @"[\d\s]+", "");
                var childNameOnly = System.Text.RegularExpressions.Regex.Replace(normalizedChildName, @"[\d\s]+", "");
                
                return queryLower.Contains(childNameLower) || 
                       childNameLower.Contains(queryLower) ||
                       normalizedQuery.Contains(normalizedChildName) ||
                       normalizedChildName.Contains(normalizedQuery) ||
                       queryNoSpace.Contains(childNameNoSpace) ||
                       childNameNoSpace.Contains(queryNoSpace) ||
                       queryNameOnly == childNameOnly ||
                       (queryNameOnly.Length > 3 && childNameOnly.Contains(queryNameOnly)) ||
                       (childNameOnly.Length > 3 && queryNameOnly.Contains(childNameOnly));
            });
            
            if (matchedChild != null)
            {
                // Chỉ lấy cho con được chỉ định
                targetChildren = new List<Account> { matchedChild };
                Console.WriteLine($"[SystemDataService] Matched child: {matchedChild.UserName} for query: {userQuery}");
            }
            else
            {
                // Không tìm thấy con match - không lấy gì cả (hoặc có thể lấy con đầu tiên)
                Console.WriteLine($"[SystemDataService] No child matched for query: {userQuery}, returning empty");
                return; // Không lấy data nếu không match
            }
        }
        else
        {
            // Không có query - chỉ lấy cho con đầu tiên (hoặc có thể lấy tất cả tùy business logic)
            targetChildren = new List<Account> { childrenList.First() };
            Console.WriteLine($"[SystemDataService] No query specified, using first child: {targetChildren.First().UserName}");
        }

        foreach (var child in targetChildren)
        {
            var studentName = child.UserName ?? "Con của bạn";

            if (intent.DataTypes.Contains("progress"))
            {
                var progress = await _courseRepository.GetParentChildLearningProgressAsync(parentId);
                structuredData.Add(new SystemDataResponse
                {
                    DataType = "progress",
                    Title = $"Tiến độ học tập của {studentName}",
                    Data = new
                    {
                        StudentName = studentName,
                        TotalStudyHours = progress.Metadata.TotalStudyHours,
                        TotalCompletedLessonItems = progress.Metadata.TotalCompletedLessonItems,
                        TotalChildren = progress.Metadata.TotalChildren
                    }
                });
            }

            if (intent.DataTypes.Contains("statistics"))
            {
                var courseStats = await _courseRepository.GetParentChildCourseStatisticsAsync(parentId);
                structuredData.Add(new SystemDataResponse
                {
                    DataType = "statistics",
                    Title = $"Thống kê khóa học của {studentName}",
                    Data = new
                    {
                        StudentName = studentName,
                        InProgressCourses = courseStats.Metadata.InProgressCourses,
                        CompletedCourses = courseStats.Metadata.CompletedCourses,
                        TotalEnrolledCourses = courseStats.Metadata.TotalEnrolledCourses
                    }
                });
            }

            if (intent.DataTypes.Contains("performance"))
            {
                var performance = await _courseRepository.GetParentChildPerformanceAsync(parentId);
                structuredData.Add(new SystemDataResponse
                {
                    DataType = "performance",
                    Title = $"Thành tích học tập của {studentName}",
                    Data = new
                    {
                        StudentName = studentName,
                        AverageScore = performance.Metadata.AverageScore,
                        TotalQuizzesTaken = performance.Metadata.TotalQuizzesTaken,
                        QuizPassRate = performance.Metadata.QuizPassRate
                    }
                });
            }

            if (intent.DataTypes.Contains("certificates"))
            {
                var certificates = await _courseRepository.GetParentChildCertificatesAsync(parentId);
                if (certificates.Certificates.Any())
                {
                    structuredData.Add(new SystemDataResponse
                    {
                        DataType = "certificates",
                        Title = $"Chứng chỉ của {studentName}",
                        Data = new
                        {
                            StudentName = studentName,
                            Certificates = certificates.Certificates
                        }
                    });
                }
            }

            if (intent.DataTypes.Contains("recommendations"))
            {
                var recommendations = await _courseRepository.GetCourseRecommendationsAsync(child.Id, limit: 5);
                if (recommendations.RecommendedCourses.Any())
                {
                    structuredData.Add(new SystemDataResponse
                    {
                        DataType = "recommendations",
                        Title = $"Khóa học gợi ý cho {studentName}",
                        Data = new
                        {
                            StudentName = studentName,
                            Courses = recommendations.RecommendedCourses
                        }
                    });
                }
            }
        }
    }

    private async Task GetManagerStructuredDataAsync(string userId, QueryIntent intent, List<SystemDataResponse> structuredData)
    {
        if (intent.DataTypes.Contains("overview"))
        {
            var overview = await _courseRepository.GetAdminOverviewAsync();
            structuredData.Add(new SystemDataResponse
            {
                DataType = "overview",
                Title = "Tổng quan hệ thống",
                Data = new
                {
                    TotalAccounts = overview.Accounts.Total,
                    TotalCourses = overview.Courses.Total,
                    TotalEnrollments = overview.Enrollments.Total,
                    TotalRevenue = overview.Revenue.Total
                }
            });
        }

        if (intent.DataTypes.Contains("user_statistics"))
        {
            var userStats = await _courseRepository.GetAdminUserStatisticsAsync();
            structuredData.Add(new SystemDataResponse
            {
                DataType = "user_statistics",
                Title = "Thống kê người dùng",
                Data = new
                {
                    TotalStudents = userStats.Metadata.TotalStudents,
                    TotalTeachers = userStats.Metadata.TotalTeachers,
                    TotalParents = userStats.Metadata.TotalParents,
                    TotalAccounts = userStats.Metadata.TotalAccounts
                }
            });
        }

        if (intent.DataTypes.Contains("statistics") && !intent.DataTypes.Contains("user_statistics") && 
            !intent.DataTypes.Contains("revenue_statistics") && !intent.DataTypes.Contains("enrollment_statistics"))
        {
            var courseStats = await _courseRepository.GetAdminCourseStatisticsAsync();
            structuredData.Add(new SystemDataResponse
            {
                DataType = "course_statistics",
                Title = "Thống kê khóa học",
                Data = new
                {
                    TotalCourses = courseStats.Metadata.TotalCourses,
                    OpenCourses = courseStats.Metadata.OpenCourses,
                    PendingCourses = courseStats.Metadata.PendingCourses,
                    NewCourses = courseStats.Metadata.NewCourses
                }
            });
        }

        if (intent.DataTypes.Contains("revenue_statistics"))
        {
            var revenueStats = await _courseRepository.GetAdminRevenueStatisticsAsync();
            structuredData.Add(new SystemDataResponse
            {
                DataType = "revenue_statistics",
                Title = "Thống kê doanh thu",
                Data = new
                {
                    TotalRevenue = revenueStats.Metadata.TotalRevenue,
                    TotalTransactions = revenueStats.Metadata.TotalTransactions,
                    AverageTransactionAmount = revenueStats.Metadata.AverageTransactionAmount
                }
            });
        }

        if (intent.DataTypes.Contains("enrollment_statistics"))
        {
            var enrollmentStats = await _courseRepository.GetAdminEnrollmentStatisticsAsync();
            structuredData.Add(new SystemDataResponse
            {
                DataType = "enrollment_statistics",
                Title = "Thống kê đăng ký",
                Data = new
                {
                    TotalEnrollments = enrollmentStats.Metadata.TotalEnrollments,
                    ActiveStudents = enrollmentStats.Metadata.ActiveStudents,
                    CompletedStudents = enrollmentStats.Metadata.CompletedStudents
                }
            });
        }
    }

    #endregion
}

