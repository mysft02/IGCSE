# Tài liệu Trợ lý Ảo AI (AI Assistant) - IGCSE Platform

Tài liệu này mô tả hệ thống Trợ lý Ảo AI cho nền tảng học tập IGCSE, bao gồm kiến trúc, pattern design, mục đích sử dụng, bối cảnh và hướng dẫn tích hợp với frontend React.

---

## Mục lục

1. [Tổng quan](#1-tổng-quan)
2. [Mục đích sử dụng](#2-mục-đích-sử-dụng)
3. [Bối cảnh sử dụng](#3-bối-cảnh-sử-dụng)
4. [Kiến trúc và Pattern Design](#4-kiến-trúc-và-pattern-design)
5. [Multi-Persona System](#5-multi-persona-system)
6. [RAG System (Retrieval Augmented Generation)](#6-rag-system-retrieval-augmented-generation)
7. [API Endpoints](#7-api-endpoints)
8. [SignalR Real-time Communication](#8-signalr-real-time-communication)
9. [Tích hợp Frontend (React)](#9-tích-hợp-frontend-react)
10. [Security & Best Practices](#10-security--best-practices)

---

## 1. Tổng quan

### 1.1. Giới thiệu

Trợ lý Ảo AI là hệ thống chat real-time thông minh được tích hợp vào nền tảng IGCSE, giúp:

- **Học viên**: Nhận hỗ trợ học tập 24/7, giải đáp thắc mắc về bài học, gợi ý học tập
- **Giáo viên**: Hỗ trợ tạo nội dung, phân tích dữ liệu học tập, tư vấn phương pháp giảng dạy
- **Phụ huynh**: Theo dõi tiến độ học tập của con, nhận lời khuyên về giáo dục

### 1.2. Tính năng chính

- ✅ **Real-time Chat**: Giao tiếp tức thời qua WebSocket (SignalR)
- ✅ **Multi-Persona**: Tính cách khác nhau cho từng loại người dùng
- ✅ **RAG System**: Trả lời chính xác dựa trên nội dung khóa học (không lạc đề)
- ✅ **Context-Aware**: Hiểu ngữ cảnh học tập của từng học viên
- ✅ **Streaming Response**: Phản hồi từng phần (typing effect)
- ✅ **Conversation History**: Lưu trữ lịch sử trò chuyện

### 1.3. Công nghệ sử dụng

- **Backend**: .NET 8, SignalR, OpenAI API
- **Database**: MySQL (chatconversation, chatmessage, assistantknowledge)
- **Frontend**: React với SignalR Client
- **AI Model**: OpenAI GPT-4 / GPT-3.5-turbo

---

## 2. Mục đích sử dụng

### 2.1. Cho Học viên (Student)

| Mục đích               | Mô tả                                                 |
| ---------------------- | ----------------------------------------------------- |
| **Hỏi đáp bài học**    | Giải thích khái niệm, công thức, ví dụ trong khóa học |
| **Hỗ trợ làm bài tập** | Gợi ý cách giải, kiểm tra đáp án, giải thích lỗi sai  |
| **Gợi ý học tập**      | Đề xuất bài học tiếp theo, lộ trình học phù hợp       |
| **Ôn tập**             | Tóm tắt nội dung, tạo câu hỏi ôn tập                  |
| **Động viên**          | Khuyến khích, theo dõi tiến độ, khen ngợi thành tích  |

### 2.2. Cho Giáo viên (Teacher)

| Mục đích              | Mô tả                                             |
| --------------------- | ------------------------------------------------- |
| **Hỗ trợ giảng dạy**  | Gợi ý phương pháp giảng dạy, tạo nội dung bài học |
| **Phân tích dữ liệu** | Phân tích kết quả học tập, điểm yếu của học viên  |
| **Tạo câu hỏi**       | Hỗ trợ tạo quiz, bài kiểm tra, câu hỏi ôn tập     |
| **Tư vấn chuyên môn** | Tư vấn về chương trình học, cải thiện khóa học    |

### 2.3. Cho Phụ huynh (Parent)

| Mục đích                    | Mô tả                                                |
| --------------------------- | ---------------------------------------------------- |
| **Theo dõi tiến độ**        | Báo cáo tiến độ học tập, điểm số, thời gian học      |
| **Giải thích chương trình** | Giải thích về nội dung khóa học, mục tiêu học tập    |
| **Lời khuyên**              | Đưa ra lời khuyên về cách hỗ trợ con học tập tại nhà |
| **Thông báo**               | Thông báo về thành tích, cảnh báo khi cần chú ý      |

---

## 3. Bối cảnh sử dụng

### 3.1. Kịch bản sử dụng

#### **Kịch bản 1: Học viên hỏi về bài học**

```
Học viên: "Tôi không hiểu về phương trình bậc hai, bạn có thể giải thích không?"

Trợ lý (StudentHelper):
- Kiểm tra khóa học đang học của học viên
- Tìm nội dung liên quan trong knowledge base (RAG)
- Giải thích dựa trên nội dung khóa học
- Đưa ra ví dụ cụ thể
- Gợi ý bài học liên quan
```

#### **Kịch bản 2: Học viên cần hỗ trợ làm bài tập**

```
Học viên: "Làm sao để giải bài này: x² + 5x + 6 = 0?"

Trợ lý:
- Không giải trực tiếp (tránh gian lận)
- Gợi ý các bước giải
- Giải thích công thức áp dụng
- Đưa ra bài tập tương tự để luyện tập
```

#### **Kịch bản 3: Giáo viên tạo nội dung**

```
Giáo viên: "Tôi muốn tạo bài giảng về đạo hàm, bạn có gợi ý không?"

Trợ lý (TeacherAssistant):
- Đề xuất cấu trúc bài giảng
- Gợi ý ví dụ thực tế
- Đề xuất bài tập phù hợp
- Tư vấn về phương pháp giảng dạy
```

#### **Kịch bản 4: Phụ huynh xem tiến độ**

```
Phụ huynh: "Con tôi học như thế nào trong tháng này?"

Trợ lý (ParentGuide):
- Báo cáo tiến độ học tập
- Số bài đã hoàn thành
- Điểm số trung bình
- Thời gian học tập
- Lời khuyên hỗ trợ con
```

### 3.2. Context Awareness

Trợ lý tự động nhận biết:

- **Khóa học đang học**: Tập trung vào nội dung liên quan
- **Tiến độ học tập**: Gợi ý phù hợp với trình độ
- **Lịch sử trò chuyện**: Hiểu ngữ cảnh câu hỏi
- **Thành tích**: Động viên dựa trên kết quả

### 3.3. Relevance Filtering

Trợ lý **KHÔNG** trả lời về:

- ❌ Tin tức, thời tiết, giải trí
- ❌ Chủ đề ngoài phạm vi IGCSE
- ❌ Câu hỏi không liên quan đến học tập

**Cơ chế**: RAG system chỉ trả về context có similarity > 0.7

---

## 4. Kiến trúc và Pattern Design

### 4.1. Kiến trúc tổng thể

```
┌─────────────────────────────────────────────────────────────┐
│                        Frontend (React)                      │
│  ┌──────────────┐  ┌──────────────┐  ┌──────────────┐      │
│  │ Chat UI      │  │ SignalR      │  │ State Mgmt   │      │
│  │ Component    │  │ Client       │  │ (Redux/Zustand)│   │
│  └──────────────┘  └──────────────┘  └──────────────┘      │
└───────────────────────────┬─────────────────────────────────┘
                            │ WebSocket (SignalR)
                            │ HTTP REST API
┌───────────────────────────▼─────────────────────────────────┐
│                    Backend (.NET 8)                          │
│  ┌──────────────────────────────────────────────────────┐  │
│  │              SignalR Hub (ChatHub)                    │  │
│  │  - Real-time message handling                        │  │
│  │  - Streaming responses                               │  │
│  │  - Connection management                              │  │
│  └──────────────────────────────────────────────────────┘  │
│  ┌──────────────────────────────────────────────────────┐  │
│  │           Assistant Service Layer                      │  │
│  │  ┌──────────────┐  ┌──────────────┐  ┌──────────────┐ │
│  │  │ Context      │  │ Persona      │  │ RAG          │ │
│  │  │ Manager      │  │ Service      │  │ Service      │ │
│  │  └──────────────┘  └──────────────┘  └──────────────┘ │
│  └──────────────────────────────────────────────────────┘  │
│  ┌──────────────────────────────────────────────────────┐  │
│  │           OpenAI Chat Service                         │  │
│  │  - GPT-4 / GPT-3.5-turbo                              │  │
│  │  - Streaming API                                      │  │
│  │  - Response generation                                │  │
│  └──────────────────────────────────────────────────────┘  │
│  ┌──────────────────────────────────────────────────────┐  │
│  │           Repository Layer                             │  │
│  │  - ChatConversationRepository                         │  │
│  │  - ChatMessageRepository                              │  │
│  │  - AssistantKnowledgeRepository                       │  │
│  └──────────────────────────────────────────────────────┘  │
└───────────────────────────┬─────────────────────────────────┘
                            │
┌───────────────────────────▼─────────────────────────────────┐
│                    Database (MySQL)                          │
│  - chatconversation                                         │
│  - chatmessage                                              │
│  - assistantknowledge (RAG)                                 │
└─────────────────────────────────────────────────────────────┘
```

### 4.2. Pattern Design

#### **A. Service Layer Pattern**

```csharp
// Service/Assistant/IAssistantService.cs
public interface IAssistantService
{
    Task<AssistantResponse> ProcessMessageAsync(
        string userId,
        string message,
        string? conversationId = null);

    Task<string> CreateConversationAsync(
        string userId,
        string initialMessage);

    Task<List<ChatMessage>> GetConversationHistoryAsync(
        string conversationId,
        int limit = 20);
}

// Service/Assistant/AssistantService.cs
public class AssistantService : IAssistantService
{
    private readonly IContextManager _contextManager;
    private readonly IPersonaService _personaService;
    private readonly IRAGService _ragService;
    private readonly IOpenAIChatService _openAIChatService;
    private readonly IChatMessageRepository _messageRepository;

    // Implementation...
}
```

#### **B. Strategy Pattern cho Persona**

```csharp
// Service/Assistant/Persona/IPersonaStrategy.cs
public interface IPersonaStrategy
{
    AssistantPersonaConfig GetConfig();
    string BuildSystemPrompt(ChatContext context);
    bool CanHandleQuery(string query, ChatContext context);
}

// Service/Assistant/Persona/StudentHelperPersona.cs
public class StudentHelperPersona : IPersonaStrategy
{
    public AssistantPersonaConfig GetConfig() => new()
    {
        Persona = AssistantPersona.StudentHelper,
        SystemPrompt = "Bạn là trợ lý học tập thân thiện...",
        Personality = "Thân thiện, nhiệt tình, kiên nhẫn",
        Tone = "Thân mật nhưng tôn trọng"
    };

    // Implementation...
}
```

#### **C. Chain of Responsibility cho Message Processing**

```csharp
// Service/Assistant/Processing/IMessageProcessor.cs
public interface IMessageProcessor
{
    Task<ProcessedMessage> ProcessAsync(MessageContext context);
    IMessageProcessor SetNext(IMessageProcessor processor);
}

// Service/Assistant/Processing/RelevanceCheckProcessor.cs
public class RelevanceCheckProcessor : IMessageProcessor
{
    public async Task<ProcessedMessage> ProcessAsync(MessageContext context)
    {
        if (!_ragService.IsRelevant(context.UserQuery, context.UserId))
        {
            return new ProcessedMessage
            {
                IsRelevant = false,
                Response = "Xin lỗi, tôi chỉ có thể hỗ trợ về IGCSE..."
            };
        }

        return await _next?.ProcessAsync(context);
    }
}
```

#### **D. Repository Pattern**

```csharp
// Repository/IRepositories/IChatConversationRepository.cs
public interface IChatConversationRepository : IBaseRepository<ChatConversation>
{
    Task<List<ChatConversation>> GetByUserIdAsync(string userId);
    Task<ChatConversation?> GetActiveConversationAsync(string userId);
}

// Repository/IRepositories/IChatMessageRepository.cs
public interface IChatMessageRepository : IBaseRepository<ChatMessage>
{
    Task<List<ChatMessage>> GetByConversationIdAsync(
        int conversationId,
        int limit = 20);

    Task<List<ChatMessage>> GetRecentMessagesAsync(
        string userId,
        int limit = 5);
}
```

### 4.3. Flow xử lý Message

```
User Message
    ↓
1. SignalR Hub receives message
    ↓
2. AssistantService.ProcessMessageAsync()
    ↓
3. ContextManager.BuildContext()
   - Get user role, current course, progress
   - RAG: Retrieve relevant knowledge
   - Get conversation history
    ↓
4. RelevanceCheckProcessor
   - Check if query is relevant (similarity > 0.7)
   - If not → Return polite decline
    ↓
5. PersonaService.GetPersona()
   - Select persona based on user role
   - Load system prompt
    ↓
6. BuildSystemPrompt()
   - System prompt + RAG context + History
   - Enforce relevance rules
    ↓
7. OpenAIChatService.ChatAsync()
   - Call OpenAI API with streaming
   - Stream response chunks
    ↓
8. SignalR Hub streams to client
   - Send chunks in real-time
   - Update UI progressively
    ↓
9. Save to Database
   - Save user message
   - Save assistant response
   - Update conversation metadata
```

---

## 5. Multi-Persona System

### 5.1. Persona Types

| Persona              | Role    | Tính cách                         | Giọng điệu               | Use Case                            |
| -------------------- | ------- | --------------------------------- | ------------------------ | ----------------------------------- |
| **StudentHelper**    | Student | Thân thiện, nhiệt tình, kiên nhẫn | Thân mật nhưng tôn trọng | Hỗ trợ học tập, giải đáp thắc mắc   |
| **TeacherAssistant** | Teacher | Chuyên nghiệp, hiệu quả, hỗ trợ   | Trang trọng, rõ ràng     | Tư vấn giảng dạy, phân tích dữ liệu |
| **ParentGuide**      | Parent  | Thông tin, hỗ trợ, rõ ràng        | Trang trọng, thân thiện  | Báo cáo tiến độ, lời khuyên         |
| **StudyBuddy**       | Student | Vui vẻ, động viên, đồng hành      | Thân mật, vui vẻ         | Động viên, tạo động lực             |
| **Tutor**            | Student | Nghiêm túc, chi tiết, chuyên sâu  | Trang trọng, kỹ lưỡng    | Giải thích sâu, ôn tập              |
| **Mentor**           | Student | Khuyên bảo, định hướng, khích lệ  | Chín chắn, khuyên bảo    | Định hướng học tập, lộ trình        |

### 5.2. Persona Configuration

```csharp
// BusinessObject/DTOs/Response/Assistant/AssistantPersonaConfig.cs
public class AssistantPersonaConfig
{
    public AssistantPersona Persona { get; set; }
    public string SystemPrompt { get; set; }
    public string Personality { get; set; }
    public string Tone { get; set; }
    public string[] AllowedTopics { get; set; }
    public Dictionary<string, object> BehaviorRules { get; set; }
}
```

### 5.3. System Prompt Template

```csharp
var systemPrompt = $@"Bạn là {persona.Personality} cho nền tảng học tập IGCSE.

TÍNH CÁCH:
- {persona.Personality}
- Giọng điệu: {persona.Tone}

NHIỆM VỤ:
- Hỗ trợ người dùng về học tập IGCSE
- CHỈ trả lời về: {string.Join(", ", persona.AllowedTopics)}
- Nếu câu hỏi không liên quan, hãy từ chối một cách lịch sự

CONTEXT HIỆN TẠI:
- Khóa học: {context.CurrentCourse?.Name}
- Tiến độ: {context.LearningProgress?.CompletionPercentage}%

KIẾN THỨC LIÊN QUAN:
{ragContext}

QUAN TRỌNG:
- CHỈ trả lời dựa trên context và kiến thức trên
- Không được bịa đặt thông tin
- Nếu không chắc chắn, hãy thừa nhận và đề xuất tìm hiểu thêm";
```

---

## 6. RAG System (Retrieval Augmented Generation)

### 6.1. RAG Architecture

```
User Query
    ↓
1. Embed Query (OpenAI Embeddings API)
   - Convert query to vector
    ↓
2. Vector Search (Cosine Similarity)
   - Search in assistantknowledge table
   - Calculate similarity scores
   - Filter: similarity > 0.7
    ↓
3. Retrieve Top-K Results
   - Get top 5-10 most relevant content
   - Include metadata (courseId, lessonId)
    ↓
4. Build Context
   - Combine retrieved content
   - Add to system prompt
    ↓
5. Generate Response
   - OpenAI Chat API with RAG context
   - Ensure response is grounded in retrieved content
```

### 6.2. Knowledge Base Indexing

```csharp
// Service/Assistant/RAG/KnowledgeIndexingService.cs
public class KnowledgeIndexingService
{
    public async Task IndexCourseContentAsync(int courseId)
    {
        var course = await _courseRepository.GetByCourseIdAsync(courseId);

        // Index course description
        await IndexContent(
            content: course.Description,
            sourceType: "Course",
            sourceId: courseId
        );

        // Index sections
        var sections = await _sectionRepository.GetByCourseIdAsync(courseId);
        foreach (var section in sections)
        {
            await IndexContent(
                content: $"{section.Name}. {section.Description}",
                sourceType: "Section",
                sourceId: section.CourseSectionId,
                metadata: new { courseId, sectionId = section.CourseSectionId }
            );

            // Index lessons
            var lessons = await _lessonRepository.GetBySectionIdAsync(section.CourseSectionId);
            foreach (var lesson in lessons)
            {
                await IndexContent(
                    content: $"{lesson.Name}. {lesson.Description}",
                    sourceType: "Lesson",
                    sourceId: lesson.LessonId,
                    metadata: new { courseId, sectionId, lessonId = lesson.LessonId }
                );
            }
        }
    }

    private async Task IndexContent(
        string content,
        string sourceType,
        int sourceId,
        object? metadata = null)
    {
        // Generate embedding
        var embedding = await _embeddingService.EmbedData(content);

        // Save to assistantknowledge
        var knowledge = new AssistantKnowledge
        {
            Content = content,
            EmbeddingData = JsonSerializer.Serialize(embedding),
            SourceType = sourceType,
            SourceId = sourceId,
            Metadata = metadata != null ? JsonSerializer.Serialize(metadata) : null,
            CreatedAt = DateTime.UtcNow
        };

        await _knowledgeRepository.AddAsync(knowledge);
    }
}
```

### 6.3. Vector Search Implementation

```csharp
// Service/Assistant/RAG/RAGService.cs
public class RAGService
{
    public async Task<List<RelevantContext>> RetrieveRelevantContext(
        string userQuery,
        string? userId = null,
        int? courseId = null)
    {
        // 1. Embed query
        var queryEmbedding = await _embeddingService.EmbedData(userQuery);
        var queryEmbeddingJson = JsonSerializer.Serialize(queryEmbedding);

        // 2. Vector search with cosine similarity
        var sql = @"
            SELECT
                k.KnowledgeId,
                k.Content,
                k.SourceType,
                k.SourceId,
                k.Metadata,
                -- Cosine similarity calculation
                (
                    SELECT SUM(v1.value * v2.value)
                    FROM JSON_TABLE(?, '$[*]' COLUMNS (value DOUBLE PATH '$')) AS v1
                    JOIN JSON_TABLE(k.EmbeddingData, '$[*]' COLUMNS (value DOUBLE PATH '$')) AS v2
                    ON v1.ordinal = v2.ordinal
                ) / (
                    SQRT(
                        (SELECT SUM(POWER(value, 2)) FROM JSON_TABLE(?, '$[*]' COLUMNS (value DOUBLE PATH '$')))
                    ) *
                    SQRT(
                        (SELECT SUM(POWER(value, 2)) FROM JSON_TABLE(k.EmbeddingData, '$[*]' COLUMNS (value DOUBLE PATH '$')))
                    )
                ) AS SimilarityScore
            FROM assistantknowledge k
            WHERE k.SourceType IN ('Course', 'Section', 'Lesson')
            AND (? IS NULL OR JSON_EXTRACT(k.Metadata, '$.courseId') = ?)
            HAVING SimilarityScore > 0.7
            ORDER BY SimilarityScore DESC
            LIMIT 10";

        // Execute query and return results
        // ...
    }
}
```

---

## 7. API Endpoints

### 7.1. REST API Endpoints

#### **GET /api/assistant/conversations**

Lấy danh sách cuộc trò chuyện của user

**Request:**

```http
GET /api/assistant/conversations
Authorization: Bearer <token>
```

**Response:**

```json
{
  "message": "Lấy danh sách cuộc trò chuyện thành công",
  "statusCode": 200,
  "data": [
    {
      "conversationId": 1,
      "title": "Hỏi về phương trình bậc hai",
      "personaType": "StudentHelper",
      "createdAt": "2024-01-15T10:30:00Z",
      "updatedAt": "2024-01-15T11:00:00Z",
      "isActive": true
    }
  ]
}
```

#### **GET /api/assistant/conversations/{conversationId}/messages**

Lấy lịch sử tin nhắn của cuộc trò chuyện

**Request:**

```http
GET /api/assistant/conversations/1/messages?limit=20
Authorization: Bearer <token>
```

**Response:**

```json
{
  "message": "Lấy lịch sử tin nhắn thành công",
  "statusCode": 200,
  "data": [
    {
      "messageId": 1,
      "content": "Tôi không hiểu về phương trình bậc hai",
      "role": "user",
      "createdAt": "2024-01-15T10:30:00Z"
    },
    {
      "messageId": 2,
      "content": "Phương trình bậc hai có dạng ax² + bx + c = 0...",
      "role": "assistant",
      "createdAt": "2024-01-15T10:30:05Z",
      "metadata": {
        "relevantKnowledgeIds": [1, 2, 3]
      }
    }
  ]
}
```

#### **POST /api/assistant/conversations**

Tạo cuộc trò chuyện mới

**Request:**

```http
POST /api/assistant/conversations
Authorization: Bearer <token>
Content-Type: application/json

{
  "initialMessage": "Xin chào, tôi cần hỗ trợ về bài học"
}
```

**Response:**

```json
{
  "message": "Tạo cuộc trò chuyện thành công",
  "statusCode": 201,
  "data": {
    "conversationId": 1,
    "title": "Xin chào, tôi cần hỗ trợ về bài học",
    "personaType": "StudentHelper",
    "createdAt": "2024-01-15T10:30:00Z"
  }
}
```

#### **DELETE /api/assistant/conversations/{conversationId}**

Xóa cuộc trò chuyện

**Request:**

```http
DELETE /api/assistant/conversations/1
Authorization: Bearer <token>
```

**Response:**

```json
{
  "message": "Xóa cuộc trò chuyện thành công",
  "statusCode": 200,
  "data": "Cuộc trò chuyện đã được xóa"
}
```

---

## 8. SignalR Real-time Communication

### 8.1. SignalR Hub Setup

```csharp
// IGCSE/Hubs/ChatHub.cs
[Authorize]
public class ChatHub : Hub
{
    private readonly IAssistantService _assistantService;
    private readonly IChatMessageRepository _messageRepository;

    public async Task SendMessage(string conversationId, string message)
    {
        var userId = Context.User?.FindFirst("AccountID")?.Value;
        if (string.IsNullOrEmpty(userId))
        {
            await Clients.Caller.SendAsync("Error", "Unauthorized");
            return;
        }

        // Save user message
        var userMessage = new ChatMessage
        {
            ConversationId = int.Parse(conversationId),
            Content = message,
            Role = "user",
            CreatedAt = DateTime.UtcNow
        };
        await _messageRepository.AddAsync(userMessage);

        // Notify typing
        await Clients.Group(conversationId).SendAsync("Typing", true);

        // Process with AI (streaming)
        await foreach (var chunk in _assistantService.ProcessMessageStreamAsync(
            userId, message, conversationId))
        {
            await Clients.Caller.SendAsync("ReceiveMessageChunk", chunk);
        }

        // Stop typing
        await Clients.Group(conversationId).SendAsync("Typing", false);
    }

    public async Task JoinConversation(string conversationId)
    {
        await Groups.AddToGroupAsync(Context.ConnectionId, conversationId);
    }

    public async Task LeaveConversation(string conversationId)
    {
        await Groups.RemoveFromGroupAsync(Context.ConnectionId, conversationId);
    }
}
```

### 8.2. Program.cs Configuration

```csharp
// IGCSE/Program.cs
builder.Services.AddSignalR();

// ...

app.MapHub<ChatHub>("/chatHub");
```

### 8.3. JWT Authentication cho SignalR

```csharp
// JWT token được truyền qua query string
// ?access_token=<token>
// Hoặc trong header (đã config trong Program.cs)
```

---

## 9. Tích hợp Frontend (React)

### 9.1. Cài đặt Dependencies

```bash
npm install @microsoft/signalr
# hoặc
yarn add @microsoft/signalr
```

### 9.2. SignalR Client Setup

```typescript
// src/services/signalr/chatHubService.ts
import * as signalR from "@microsoft/signalr";

class ChatHubService {
  private connection: signalR.HubConnection | null = null;

  constructor(private token: string) {}

  async connect(): Promise<void> {
    this.connection = new signalR.HubConnectionBuilder()
      .withUrl(`${process.env.REACT_APP_API_URL}/chatHub`, {
        accessTokenFactory: () => this.token,
        // Hoặc truyền qua query string
        // skipNegotiation: true,
        // transport: signalR.HttpTransportType.WebSockets
      })
      .withAutomaticReconnect()
      .build();

    // Register event handlers
    this.connection.on("ReceiveMessageChunk", (chunk: string) => {
      // Handle streaming message chunk
      this.onMessageChunk?.(chunk);
    });

    this.connection.on("Typing", (isTyping: boolean) => {
      this.onTyping?.(isTyping);
    });

    this.connection.on("Error", (error: string) => {
      this.onError?.(error);
    });

    await this.connection.start();
  }

  async sendMessage(conversationId: string, message: string): Promise<void> {
    if (!this.connection) {
      throw new Error("Connection not established");
    }

    await this.connection.invoke("SendMessage", conversationId, message);
  }

  async joinConversation(conversationId: string): Promise<void> {
    if (!this.connection) {
      throw new Error("Connection not established");
    }

    await this.connection.invoke("JoinConversation", conversationId);
  }

  async disconnect(): Promise<void> {
    if (this.connection) {
      await this.connection.stop();
      this.connection = null;
    }
  }

  // Event handlers
  onMessageChunk?: (chunk: string) => void;
  onTyping?: (isTyping: boolean) => void;
  onError?: (error: string) => void;
}

export default ChatHubService;
```

### 9.3. React Hook cho Chat

```typescript
// src/hooks/useChat.ts
import { useState, useEffect, useCallback, useRef } from "react";
import ChatHubService from "../services/signalr/chatHubService";
import { getAuthToken } from "../utils/auth";

interface Message {
  messageId?: number;
  content: string;
  role: "user" | "assistant";
  createdAt?: string;
}

export const useChat = (conversationId: string | null) => {
  const [messages, setMessages] = useState<Message[]>([]);
  const [isTyping, setIsTyping] = useState(false);
  const [isConnected, setIsConnected] = useState(false);
  const [currentMessage, setCurrentMessage] = useState("");
  const chatHubRef = useRef<ChatHubService | null>(null);
  const streamingMessageRef = useRef<string>("");

  useEffect(() => {
    const token = getAuthToken();
    if (!token || !conversationId) return;

    const chatHub = new ChatHubService(token);
    chatHubRef.current = chatHub;

    // Setup event handlers
    chatHub.onMessageChunk = (chunk: string) => {
      streamingMessageRef.current += chunk;
      setMessages((prev) => {
        const lastMessage = prev[prev.length - 1];
        if (lastMessage?.role === "assistant" && !lastMessage.messageId) {
          // Update streaming message
          return [
            ...prev.slice(0, -1),
            { ...lastMessage, content: streamingMessageRef.current },
          ];
        } else {
          // New assistant message
          return [...prev, { role: "assistant" as const, content: chunk }];
        }
      });
    };

    chatHub.onTyping = (typing: boolean) => {
      setIsTyping(typing);
    };

    chatHub.onError = (error: string) => {
      console.error("Chat error:", error);
    };

    // Connect
    chatHub.connect().then(() => {
      setIsConnected(true);
      if (conversationId) {
        chatHub.joinConversation(conversationId);
      }
    });

    return () => {
      chatHub.disconnect();
    };
  }, [conversationId]);

  const sendMessage = useCallback(
    async (message: string) => {
      if (!chatHubRef.current || !conversationId || !message.trim()) return;

      // Add user message to UI
      const userMessage: Message = {
        content: message,
        role: "user",
      };
      setMessages((prev) => [...prev, userMessage]);
      setCurrentMessage("");

      // Reset streaming message
      streamingMessageRef.current = "";

      // Add placeholder for assistant response
      setMessages((prev) => [...prev, { role: "assistant", content: "" }]);

      try {
        await chatHubRef.current.sendMessage(conversationId, message);
      } catch (error) {
        console.error("Failed to send message:", error);
        // Remove placeholder on error
        setMessages((prev) => prev.slice(0, -1));
      }
    },
    [conversationId]
  );

  return {
    messages,
    isTyping,
    isConnected,
    currentMessage,
    setCurrentMessage,
    sendMessage,
  };
};
```

### 9.4. React Component

```typescript
// src/components/Chat/ChatWindow.tsx
import React from "react";
import { useChat } from "../../hooks/useChat";
import "./ChatWindow.css";

interface ChatWindowProps {
  conversationId: string;
}

const ChatWindow: React.FC<ChatWindowProps> = ({ conversationId }) => {
  const {
    messages,
    isTyping,
    isConnected,
    currentMessage,
    setCurrentMessage,
    sendMessage,
  } = useChat(conversationId);

  const messagesEndRef = React.useRef<HTMLDivElement>(null);

  const scrollToBottom = () => {
    messagesEndRef.current?.scrollIntoView({ behavior: "smooth" });
  };

  React.useEffect(() => {
    scrollToBottom();
  }, [messages]);

  const handleSubmit = (e: React.FormEvent) => {
    e.preventDefault();
    if (currentMessage.trim()) {
      sendMessage(currentMessage);
    }
  };

  return (
    <div className="chat-window">
      <div className="chat-messages">
        {messages.map((message, index) => (
          <div
            key={index}
            className={`message ${
              message.role === "user" ? "user-message" : "assistant-message"
            }`}
          >
            <div className="message-content">
              {message.content || (isTyping && "...")}
            </div>
          </div>
        ))}
        {isTyping && (
          <div className="typing-indicator">
            <span></span>
            <span></span>
            <span></span>
          </div>
        )}
        <div ref={messagesEndRef} />
      </div>

      <form onSubmit={handleSubmit} className="chat-input-form">
        <input
          type="text"
          value={currentMessage}
          onChange={(e) => setCurrentMessage(e.target.value)}
          placeholder="Nhập câu hỏi của bạn..."
          disabled={!isConnected}
        />
        <button type="submit" disabled={!isConnected || !currentMessage.trim()}>
          Gửi
        </button>
      </form>

      {!isConnected && <div className="connection-status">Đang kết nối...</div>}
    </div>
  );
};

export default ChatWindow;
```

### 9.5. CSS Styling

```css
/* src/components/Chat/ChatWindow.css */
.chat-window {
  display: flex;
  flex-direction: column;
  height: 100%;
  max-width: 800px;
  margin: 0 auto;
  border: 1px solid #e0e0e0;
  border-radius: 8px;
  overflow: hidden;
}

.chat-messages {
  flex: 1;
  overflow-y: auto;
  padding: 16px;
  background-color: #f5f5f5;
}

.message {
  margin-bottom: 16px;
  display: flex;
}

.user-message {
  justify-content: flex-end;
}

.assistant-message {
  justify-content: flex-start;
}

.message-content {
  max-width: 70%;
  padding: 12px 16px;
  border-radius: 12px;
  word-wrap: break-word;
}

.user-message .message-content {
  background-color: #007bff;
  color: white;
}

.assistant-message .message-content {
  background-color: white;
  color: #333;
  box-shadow: 0 1px 2px rgba(0, 0, 0, 0.1);
}

.chat-input-form {
  display: flex;
  padding: 16px;
  background-color: white;
  border-top: 1px solid #e0e0e0;
}

.chat-input-form input {
  flex: 1;
  padding: 12px;
  border: 1px solid #e0e0e0;
  border-radius: 24px;
  outline: none;
}

.chat-input-form button {
  margin-left: 8px;
  padding: 12px 24px;
  background-color: #007bff;
  color: white;
  border: none;
  border-radius: 24px;
  cursor: pointer;
}

.chat-input-form button:disabled {
  background-color: #ccc;
  cursor: not-allowed;
}

.typing-indicator {
  display: flex;
  gap: 4px;
  padding: 12px;
}

.typing-indicator span {
  width: 8px;
  height: 8px;
  border-radius: 50%;
  background-color: #999;
  animation: typing 1.4s infinite;
}

.typing-indicator span:nth-child(2) {
  animation-delay: 0.2s;
}

.typing-indicator span:nth-child(3) {
  animation-delay: 0.4s;
}

@keyframes typing {
  0%,
  60%,
  100% {
    transform: translateY(0);
  }
  30% {
    transform: translateY(-10px);
  }
}

.connection-status {
  padding: 8px;
  text-align: center;
  background-color: #fff3cd;
  color: #856404;
  font-size: 14px;
}
```

### 9.6. Environment Variables

```env
# .env
REACT_APP_API_URL=http://localhost:5000
REACT_APP_SIGNALR_URL=http://localhost:5000
```

---

## 10. Security & Best Practices

### 10.1. Security

- ✅ **JWT Authentication**: Tất cả requests yêu cầu valid JWT token
- ✅ **Role-based Access**: Persona được chọn dựa trên role của user
- ✅ **Rate Limiting**: Giới hạn số lượng requests để tránh abuse
- ✅ **Input Validation**: Validate và sanitize user input
- ✅ **Content Filtering**: Lọc nội dung không phù hợp

### 10.2. Best Practices

1. **Error Handling**: Xử lý lỗi gracefully, hiển thị thông báo rõ ràng
2. **Reconnection**: Tự động reconnect khi mất kết nối
3. **Loading States**: Hiển thị loading/typing indicators
4. **Message History**: Lưu và hiển thị lịch sử trò chuyện
5. **Optimistic Updates**: Cập nhật UI ngay lập tức, sync sau
6. **Debouncing**: Debounce typing indicators
7. **Memory Management**: Cleanup connections khi unmount

### 10.3. Performance Optimization

- **Streaming**: Sử dụng streaming để giảm latency
- **Pagination**: Phân trang lịch sử tin nhắn
- **Caching**: Cache conversation history
- **Lazy Loading**: Load messages khi cần

---

## 11. Testing

### 11.1. Unit Tests

```csharp
// Test AssistantService
[Fact]
public async Task ProcessMessageAsync_ShouldReturnRelevantResponse()
{
    // Arrange
    var userId = "test-user";
    var message = "Giải thích về phương trình bậc hai";

    // Act
    var response = await _assistantService.ProcessMessageAsync(userId, message);

    // Assert
    Assert.True(response.IsRelevant);
    Assert.Contains("phương trình", response.Content, StringComparison.OrdinalIgnoreCase);
}
```

### 11.2. Integration Tests

```csharp
// Test SignalR Hub
[Fact]
public async Task ChatHub_SendMessage_ShouldStreamResponse()
{
    // Test SignalR connection and message streaming
}
```

### 11.3. Frontend Tests

```typescript
// Test React Hook
describe("useChat", () => {
  it("should send message and receive response", async () => {
    // Test chat functionality
  });
});
```

---

## 12. Monitoring & Analytics

### 12.1. Metrics to Track

- Số lượng conversations mỗi ngày
- Số lượng messages mỗi conversation
- Response time của AI
- Relevance rate (tỷ lệ câu hỏi relevant)
- User satisfaction (nếu có rating)

### 12.2. Logging

```csharp
// Log important events
_logger.LogInformation("User {UserId} sent message in conversation {ConversationId}",
    userId, conversationId);
_logger.LogWarning("Query not relevant: {Query}", userQuery);
```

---

## 13. Deployment Checklist

- [ ] Database migration (ticket-76.yaml)
- [ ] Environment variables configured
- [ ] OpenAI API key set
- [ ] SignalR CORS configured
- [ ] Rate limiting enabled
- [ ] Monitoring setup
- [ ] Error tracking (Sentry, etc.)
- [ ] Frontend build và deploy

---

**Tài liệu này sẽ được cập nhật thường xuyên khi có thay đổi trong hệ thống.**
