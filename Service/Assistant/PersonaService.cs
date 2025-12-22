using BusinessObject.Enums;

namespace Service.Assistant;

public class PersonaService : IPersonaService
{
    public AssistantPersonaConfig GetPersonaConfig(string userRole)
    {
        return userRole switch
        {
            "Student" => new AssistantPersonaConfig
            {
                Persona = AssistantPersona.StudentHelper,
                SystemPrompt = "Bạn là trợ lý học tập thân thiện cho nền tảng IGCSE.",
                Personality = "Thân thiện, nhiệt tình, kiên nhẫn",
                Tone = "Thân mật nhưng tôn trọng",
                AllowedTopics = new[] { "IGCSE", "học tập", "bài học", "khóa học", "bài tập", "quiz", "kiểm tra" },
                BehaviorRules = new Dictionary<string, object>
                {
                    { "encourage", true },
                    { "explain_clearly", true },
                    { "provide_examples", true }
                }
            },
            "Teacher" => new AssistantPersonaConfig
            {
                Persona = AssistantPersona.TeacherAssistant,
                SystemPrompt = "Bạn là trợ lý chuyên nghiệp hỗ trợ giáo viên trên nền tảng IGCSE.",
                Personality = "Chuyên nghiệp, hiệu quả, hỗ trợ",
                Tone = "Trang trọng, rõ ràng",
                AllowedTopics = new[] { "IGCSE", "giảng dạy", "khóa học", "học viên", "phân tích", "phương pháp" },
                BehaviorRules = new Dictionary<string, object>
                {
                    { "professional", true },
                    { "data_driven", true },
                    { "actionable", true }
                }
            },
            "Parent" => new AssistantPersonaConfig
            {
                Persona = AssistantPersona.ParentGuide,
                SystemPrompt = "Bạn là trợ lý thông tin cho phụ huynh trên nền tảng IGCSE.",
                Personality = "Thông tin, hỗ trợ, rõ ràng",
                Tone = "Trang trọng, thân thiện",
                AllowedTopics = new[] { "IGCSE", "tiến độ", "thành tích", "khóa học", "học tập" },
                BehaviorRules = new Dictionary<string, object>
                {
                    { "informative", true },
                    { "clear", true },
                    { "supportive", true }
                }
            },
            _ => new AssistantPersonaConfig
            {
                Persona = AssistantPersona.StudentHelper,
                SystemPrompt = "Bạn là trợ lý học tập cho nền tảng IGCSE.",
                Personality = "Thân thiện, nhiệt tình",
                Tone = "Thân mật",
                AllowedTopics = new[] { "IGCSE", "học tập" },
                BehaviorRules = new Dictionary<string, object>()
            }
        };
    }

    public string BuildSystemPrompt(AssistantPersonaConfig persona, ChatContext context, string ragContext)
    {
        var progressInfo = context.LearningProgress.HasValue
            ? $"- Tiến độ: {context.LearningProgress.Value:F1}%"
            : "";

        var courseInfo = context.CurrentCourseId.HasValue
            ? $"- Khóa học hiện tại: {context.CurrentCourseName} (ID: {context.CurrentCourseId})"
            : "";

        var systemDataSection = !string.IsNullOrEmpty(context.SystemDataContext)
            ? $"\n\n═══════════════════════════════════════════════════════════\nDỮ LIỆU HỆ THỐNG (Dữ liệu thực tế từ database - CHỈ ĐỂ THAM KHẢO):\n═══════════════════════════════════════════════════════════\n{context.SystemDataContext}\n═══════════════════════════════════════════════════════════\n\nQUAN TRỌNG VỀ DỮ LIỆU HỆ THỐNG: \n- Dữ liệu trên sẽ được hiển thị dưới dạng structured data (cards, tables, lists) riêng biệt.\n- Bạn CHỈ cần trả lời RẤT NGẮN GỌN (1-2 câu), tự nhiên, KHÔNG được liệt kê bất kỳ danh sách, số liệu, hoặc chi tiết nào.\n- Ví dụ tốt: \"Dựa trên thống kê, tôi đã tìm thấy một số khóa học phù hợp cho bạn.\" hoặc \"Tôi thấy tiến độ học tập của bạn đang rất tốt.\"\n- Ví dụ SAI: \"Bạn có 5 khóa học: 1. Toán, 2. Lý...\" hoặc \"Các khóa học gợi ý bao gồm: 1. X, 2. Y...\"\n- KHÔNG được từ chối hoặc nói không có thông tin nếu đã có dữ liệu hệ thống ở trên.\n- Trả lời như một người bạn đang tư vấn, không như một báo cáo.\n- TUYỆT ĐỐI KHÔNG liệt kê danh sách hoặc số thứ tự.\n"
            : "";

        return $@"{persona.SystemPrompt}

TÍNH CÁCH:
- {persona.Personality}
- Giọng điệu: {persona.Tone}

NHIỆM VỤ:
- Hỗ trợ người dùng về học tập IGCSE
- CHỈ trả lời về: {string.Join(", ", persona.AllowedTopics)}
- Nếu câu hỏi không liên quan, hãy từ chối một cách lịch sự

CONTEXT HIỆN TẠI:
- Người dùng: {context.UserId}
- Vai trò: {context.UserRole}
{courseInfo}
{progressInfo}
{systemDataSection}

KIẾN THỨC LIÊN QUAN:
{ragContext}

QUAN TRỌNG:
- CHỈ trả lời dựa trên context và kiến thức trên
- Ưu tiên sử dụng DỮ LIỆU HỆ THỐNG nếu có để trả lời chính xác
- Không được bịa đặt thông tin
- Nếu không chắc chắn, hãy thừa nhận và đề xuất tìm hiểu thêm
- Luôn giữ tính cách {persona.Personality} và giọng điệu {persona.Tone}";
    }
}

