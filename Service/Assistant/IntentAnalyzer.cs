using Service.OpenAI;
using System.Text.Json;

namespace Service.Assistant;

public class IntentAnalyzer : IIntentAnalyzer
{
    private readonly OpenAIEmbeddingsApiService _embeddingsService;
    private readonly OpenAIChatService _chatService;

    // Intent patterns - key là data type, value là list các patterns
    private static readonly Dictionary<string, List<string>> IntentPatterns = new()
    {
        // Analytics/Statistics
        ["analytics"] = new() { "thống kê khóa học", "analytics", "doanh thu", "tổng quan", "khóa học của tôi", "khóa học của bạn", "thống kê" },
        ["statistics"] = new() { "thống kê", "số khóa học", "số học viên", "số giáo viên", "thống kê người dùng" },
        
        // Top/Best
        ["top_courses"] = new() { "khóa học tốt nhất", "khóa học phổ biến", "khóa học nhiều học viên", "top courses", "khóa học hay nhất", "khóa học tốt", "khóa học xuất sắc", "khóa học nổi bật", "khóa học được yêu thích", "khóa học ổn định" },
        ["top_students"] = new() { "học viên tiềm năng", "học viên giỏi", "top students", "học viên xuất sắc", "học viên tốt nhất" },
        
        // Progress
        ["progress"] = new() { "tiến độ học tập", "progress", "học được bao nhiêu", "đã học", "tiến độ", "học tập", "tiến độ con", "con học thế nào", "progress của con", "con học" },
        
        // Recommendations
        ["recommendations"] = new() { "gợi ý khóa học", "nên học gì", "phù hợp", "recommendation", "gợi ý", "nên học", "nên học khoá học nào", "nên học khóa học nào", "nên học gì trước", "khóa học nào phù hợp", "khóa học nào tốt", "nên đăng ký khóa học nào" },
        
        // Performance
        ["performance"] = new() { "thành tích con", "điểm số con", "performance", "thành tích", "điểm số", "điểm" },
        
        // Certificates
        ["certificates"] = new() { "chứng chỉ con", "certificate", "chứng chỉ" },
        
        // Manager intents
        ["overview"] = new() { "tổng quan hệ thống", "overview", "tổng hợp", "tổng quan" },
        ["user_statistics"] = new() { "thống kê người dùng", "số học viên", "số giáo viên", "người dùng", "user" },
        ["revenue_statistics"] = new() { "doanh thu", "revenue", "tiền", "money" },
        ["enrollment_statistics"] = new() { "đăng ký", "enrollment", "số đăng ký" }
    };

    public IntentAnalyzer(
        OpenAIEmbeddingsApiService embeddingsService,
        OpenAIChatService chatService)
    {
        _embeddingsService = embeddingsService;
        _chatService = chatService;
    }

    public async Task<QueryIntent> AnalyzeIntentAsync(string userQuery, string userRole)
    {
        // Strategy 1: Fast path - Simple keyword matching với fuzzy logic
        var fastIntent = AnalyzeIntentFast(userQuery, userRole);
        if (fastIntent.NeedsSystemData && fastIntent.RelevanceScore > 0.7m)
        {
            return fastIntent;
        }

        // Strategy 2: AI-based intent classification (nếu fast path không chắc chắn)
        if (fastIntent.RelevanceScore < 0.5m)
        {
            return await AnalyzeIntentWithAIAsync(userQuery, userRole);
        }

        return fastIntent;
    }

    /// <summary>
    /// Fast path: Phân tích intent bằng fuzzy matching với patterns
    /// </summary>
    private QueryIntent AnalyzeIntentFast(string userQuery, string userRole)
    {
        var intent = new QueryIntent { NeedsSystemData = false, RelevanceScore = 0.0m };
        var queryLower = userQuery.ToLowerInvariant();

        // Tính similarity với tất cả patterns
        var matches = new List<(string DataType, decimal Score)>();

        foreach (var (dataType, patterns) in IntentPatterns)
        {
            foreach (var pattern in patterns)
            {
                // Simple fuzzy matching
                var similarity = CalculateSimpleSimilarity(queryLower, pattern);
                if (similarity > 0.3m) // Threshold thấp để bắt được nhiều trường hợp
                {
                    matches.Add((dataType, similarity));
                }
            }
        }

        if (matches.Any())
        {
            // Lấy các data types có score cao nhất (có thể có nhiều)
            var topMatches = matches
                .Where(m => m.Score > 0.5m) // Chỉ lấy những match chắc chắn
                .GroupBy(m => m.DataType)
                .Select(g => new { DataType = g.Key, MaxScore = g.Max(m => m.Score) })
                .OrderByDescending(x => x.MaxScore)
                .ToList();

            if (topMatches.Any())
            {
                intent.NeedsSystemData = true;
                intent.RelevanceScore = topMatches.First().MaxScore;
                intent.DataTypes = topMatches.Select(m => m.DataType).Distinct().ToList();
            }
            else
            {
                // Nếu không có match chắc chắn, lấy match tốt nhất
                var bestMatch = matches.OrderByDescending(m => m.Score).First();
                intent.NeedsSystemData = true;
                intent.RelevanceScore = bestMatch.Score;
                intent.DataTypes.Add(bestMatch.DataType);
            }
        }

        return intent;
    }

    /// <summary>
    /// AI-based intent classification (fallback khi fast path không chắc chắn)
    /// </summary>
    private async Task<QueryIntent> AnalyzeIntentWithAIAsync(string userQuery, string userRole)
    {
        var systemPrompt = $@"Bạn là hệ thống phân tích intent cho AI Assistant.

Nhiệm vụ: Phân tích câu hỏi của user và xác định loại dữ liệu hệ thống cần lấy.

Vai trò user: {userRole}

Các loại dữ liệu có thể:
- analytics: Thống kê tổng quan (số khóa học, doanh thu, v.v.)
- top_courses: Top khóa học phổ biến/tốt nhất
- top_students: Top học viên tiềm năng
- progress: Tiến độ học tập
- statistics: Thống kê chi tiết
- recommendations: Gợi ý khóa học
- performance: Thành tích, điểm số
- certificates: Chứng chỉ
- overview: Tổng quan hệ thống (cho Manager)
- user_statistics: Thống kê người dùng (cho Manager)
- revenue_statistics: Thống kê doanh thu (cho Manager)
- enrollment_statistics: Thống kê đăng ký (cho Manager)

Trả về JSON format:
{{
  ""dataTypes"": [""analytics"", ""top_courses""],
  ""relevanceScore"": 0.8,
  ""needsSystemData"": true
}}

Chỉ trả về JSON, không có text khác.";

        try
        {
            var response = await _chatService.ChatAsync(systemPrompt, userQuery);
            
            // Parse JSON response
            var jsonStart = response.IndexOf('{');
            var jsonEnd = response.LastIndexOf('}');
            if (jsonStart >= 0 && jsonEnd > jsonStart)
            {
                var json = response.Substring(jsonStart, jsonEnd - jsonStart + 1);
                var intent = JsonSerializer.Deserialize<QueryIntent>(json, new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                });
                
                if (intent != null)
                {
                    return intent;
                }
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error in AI intent analysis: {ex.Message}");
        }

        // Fallback: return empty intent
        return new QueryIntent { NeedsSystemData = false, RelevanceScore = 0.0m };
    }

    /// <summary>
    /// Tính similarity đơn giản dựa trên từ khóa chung
    /// </summary>
    private decimal CalculateSimpleSimilarity(string query, string pattern)
    {
        var queryWords = query.Split(new[] { ' ', ',', '.', '?', '!' }, StringSplitOptions.RemoveEmptyEntries);
        var patternWords = pattern.Split(new[] { ' ', ',', '.', '?', '!' }, StringSplitOptions.RemoveEmptyEntries);

        var commonWords = queryWords.Intersect(patternWords, StringComparer.OrdinalIgnoreCase).Count();
        var totalWords = Math.Max(queryWords.Length, patternWords.Length);

        if (totalWords == 0) return 0m;

        var wordSimilarity = (decimal)commonWords / totalWords;

        // Bonus nếu query chứa pattern hoặc ngược lại
        var containsBonus = 0m;
        if (query.Contains(pattern, StringComparison.OrdinalIgnoreCase))
        {
            containsBonus = 0.3m;
        }
        else if (pattern.Contains(query, StringComparison.OrdinalIgnoreCase))
        {
            containsBonus = 0.2m;
        }

        return Math.Min(1.0m, wordSimilarity + containsBonus);
    }
}

