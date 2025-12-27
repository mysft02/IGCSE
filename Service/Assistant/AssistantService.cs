using BusinessObject.DTOs.Response.Assistant;
using BusinessObject.Model;
using Repository.IRepositories;
using System.Text.Json;
using Service.OpenAI;
using Common.Utils;
using Org.BouncyCastle.Asn1.Cmp;

namespace Service.Assistant;

public class AssistantService : IAssistantService
{
    private readonly IContextManager _contextManager;
    private readonly IPersonaService _personaService;
    private readonly IRAGService _ragService;
    private readonly OpenAIChatService _openAIChatService;
    private readonly IChatMessageRepository _messageRepository;
    private readonly IChatConversationRepository _conversationRepository;
    private readonly ISystemDataService _systemDataService;
    private readonly IAssistantKnowledgeRepository _knowledgeRepository;
    private readonly OpenAIEmbeddingsApiService _embeddingsService;
    private readonly JsonSerializerOptions _jsonOptions;

    public AssistantService(
        IContextManager contextManager,
        IPersonaService personaService,
        IRAGService ragService,
        OpenAIChatService openAIChatService,
        IChatMessageRepository messageRepository,
        IChatConversationRepository conversationRepository,
        ISystemDataService systemDataService,
        IAssistantKnowledgeRepository knowledgeRepository,
        OpenAIEmbeddingsApiService embeddingsService)
    {
        _contextManager = contextManager;
        _personaService = personaService;
        _ragService = ragService;
        _openAIChatService = openAIChatService;
        _messageRepository = messageRepository;
        _conversationRepository = conversationRepository;
        _systemDataService = systemDataService;
        _knowledgeRepository = knowledgeRepository;
        _embeddingsService = embeddingsService;
        
        // Configure JSON options với camelCase
        _jsonOptions = new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            WriteIndented = false
        };
    }

    public async Task<AssistantResponse> ProcessMessageAsync(
        string userId,
        string message,
        string? conversationId = null,
        string? userRole = null)
    {
        // 1. Check relevance
        var isRelevant = await _ragService.IsRelevantAsync(message, userId);
        if (!isRelevant)
        {
            return new AssistantResponse
            {
                Content = "Xin lỗi, tôi chỉ có thể hỗ trợ về nội dung liên quan đến IGCSE và học tập. Bạn có thể hỏi tôi về khóa học, bài học, hoặc bài tập không?",
                IsRelevant = false
            };
        }

        // 2. Build context (include userQuery for system data)
        var context = await _contextManager.BuildContextAsync(userId, userRole, message);

        // 3. Get persona
        var persona = _personaService.GetPersonaConfig(context.UserRole);

        // 4. Retrieve relevant knowledge (RAG)
        var relevantContexts = await _ragService.RetrieveRelevantContextAsync(
            message,
            userId,
            context.CurrentCourseId
        );

        var ragContext = string.Join("\n\n", relevantContexts.Select(c => c.Content));

        // 5. Build system prompt
        var systemPrompt = _personaService.BuildSystemPrompt(persona, context, ragContext);

        // 6. Get conversation history
        var history = new List<string>();
        if (!string.IsNullOrEmpty(conversationId) && int.TryParse(conversationId, out var convId))
        {
            var messages = await _messageRepository.GetByConversationIdAsync(convId, limit: 10);
            history = messages.Select(m => m.Content).ToList();
        }

        // 7. Get structured system data (nếu có) - lấy trước để có thể lưu vào metadata
        var structuredData = await _systemDataService.GetStructuredSystemDataAsync(userId, context.UserRole ?? "Student", message);

        // 8. Call OpenAI
        var response = await _openAIChatService.ChatAsync(systemPrompt, message, history);

        // 9. Save messages
        int finalConversationId;
        if (string.IsNullOrEmpty(conversationId) || !int.TryParse(conversationId, out finalConversationId))
        {
            // Create new conversation
            var conversation = new ChatConversation
            {
                UserId = userId,
                Title = message.Length > 50 ? message.Substring(0, 50) + "..." : message,
                PersonaType = persona.Persona.ToString(),
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow,
                IsActive = true
            };
            conversation = await _conversationRepository.AddAsync(conversation);
            finalConversationId = conversation.ConversationId;
        }
        else
        {
            // Update existing conversation
            var conversation = await _conversationRepository.GetByIdAsync(finalConversationId);
            if (conversation != null)
            {
                conversation.UpdatedAt = DateTime.UtcNow;
                await _conversationRepository.UpdateAsync(conversation);
            }
        }

        // Save user message
        var userMessage = new ChatMessage
        {
            ConversationId = finalConversationId,
            Content = message,
            Role = "user",
            CreatedAt = DateTime.UtcNow,
            IsRelevant = true
        };
        await _messageRepository.AddAsync(userMessage);

        // Build JSON response với text và structured data
        var jsonResponse = new
        {
            text = response, // Text response ngắn gọn từ AI
            systemData = structuredData.Any() ? structuredData : null
        };
        
        var jsonString = JsonSerializer.Serialize(jsonResponse, _jsonOptions);
        
        // Save assistant response: Content là JSON string, Metadata chứa RelevantKnowledgeIds
        var metadata = new
        {
            RelevantKnowledgeIds = relevantContexts.Select(c => c.KnowledgeId).ToList()
        };
        
        var assistantMessage = new ChatMessage
        {
            ConversationId = finalConversationId,
            Content = jsonString, // Lưu JSON string thay vì text thuần
            Role = "assistant",
            CreatedAt = DateTime.UtcNow,
            IsRelevant = true,
            Metadata = JsonSerializer.Serialize(metadata, _jsonOptions)
        };
        await _messageRepository.AddAsync(assistantMessage);

        // 10. Lưu Q&A vào knowledge base để học từ các cuộc trò chuyện
        _ = Task.Run(async () =>
        {
            try
            {
                await SaveConversationToKnowledgeAsync(message, response, finalConversationId, userId);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error saving conversation to knowledge base: {ex.Message}");
                // Không throw - không block response
            }
        });

        // Return response: Content là JSON string nếu có structured data, text thuần nếu không
        var responseContent = structuredData.Any() 
            ? jsonString  // Trả về JSON string để frontend parse
            : response;   // Trả về text thuần nếu không có structured data

        return new AssistantResponse
        {
            Content = responseContent,
            IsRelevant = true,
            ConversationId = finalConversationId.ToString(),
            RelevantKnowledgeIds = relevantContexts.Select(c => c.KnowledgeId).ToList(),
            SystemData = structuredData.Any() ? structuredData : null
        };
    }

    public async IAsyncEnumerable<string> ProcessMessageStreamAsync(
        string userId,
        string message,
        string? conversationId = null,
        string? userRole = null)
    {
        // Similar to ProcessMessageAsync but with streaming
        // Check relevance - but don't block if no knowledge base data yet
        var relevantContexts = await _ragService.RetrieveRelevantContextAsync(message, userId, null);
        var isRelevant = relevantContexts.Any();
        
        if (!isRelevant)
        {
            Console.WriteLine($"No relevant contexts found for query: {message}");
            // Continue anyway - might be first time setup or knowledge base is empty
        }

        var context = await _contextManager.BuildContextAsync(userId, userRole, message);
        Console.WriteLine($"Context built - UserId: {userId}, Role: {context.UserRole}, CourseId: {context.CurrentCourseId}");
        
        var persona = _personaService.GetPersonaConfig(context.UserRole);
        Console.WriteLine($"Persona selected: {persona.Persona}");
        
        // Get relevant contexts again with course context
        relevantContexts = await _ragService.RetrieveRelevantContextAsync(message, userId, context.CurrentCourseId);
        var ragContext = relevantContexts.Any() 
            ? string.Join("\n\n", relevantContexts.Select(c => c.Content))
            : "Chưa có kiến thức liên quan trong hệ thống. Hãy trả lời dựa trên kiến thức chung về IGCSE.";
        
        Console.WriteLine($"RAG context length: {ragContext.Length}, Relevant contexts count: {relevantContexts.Count}");
        
        var systemPrompt = _personaService.BuildSystemPrompt(persona, context, ragContext);

        // Get structured system data (nếu có) - lấy trước để có thể lưu vào metadata
        var structuredData = await _systemDataService.GetStructuredSystemDataAsync(userId, context.UserRole ?? "Student", message);

        // Get or create conversation
        int finalConversationId;
        if (string.IsNullOrEmpty(conversationId) || !int.TryParse(conversationId, out finalConversationId))
        {
            var conversation = new ChatConversation
            {
                UserId = userId,
                Title = message.Length > 50 ? message.Substring(0, 50) + "..." : message,
                PersonaType = persona.Persona.ToString(),
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow,
                IsActive = true
            };
            conversation = await _conversationRepository.AddAsync(conversation);
            finalConversationId = conversation.ConversationId;
            Console.WriteLine($"Created new conversation: {finalConversationId}");
        }
        else
        {
            Console.WriteLine($"Using existing conversation: {finalConversationId}");
        }

        // Save user message
        var userMessage = new ChatMessage
        {
            ConversationId = finalConversationId,
            Content = message,
            Role = "user",
            CreatedAt = DateTime.UtcNow,
            IsRelevant = isRelevant
        };
        await _messageRepository.AddAsync(userMessage);
        Console.WriteLine($"Saved user message to conversation {finalConversationId}");

        // Stream response
        Console.WriteLine("Calling OpenAI Chat API...");
        var fullResponse = "";
        
        // Stream response - yield return must be outside try-catch
        await foreach (var chunk in _openAIChatService.ChatStreamAsync(systemPrompt, message))
        {
            fullResponse += chunk;
            yield return chunk;
        }
        
        Console.WriteLine($"Received full response from OpenAI, length: {fullResponse.Length}");

        // Build JSON response với text và structured data - wrap in try-catch for error handling
        try
        {
            var jsonResponse = new
            {
                text = fullResponse, // Text response ngắn gọn từ AI
                systemData = structuredData.Any() ? structuredData : null
            };
            
            var jsonString = JsonSerializer.Serialize(jsonResponse, _jsonOptions);
            
            // Save assistant response: Content là JSON string, Metadata chứa RelevantKnowledgeIds
            var metadata = new
            {
                RelevantKnowledgeIds = relevantContexts.Select(c => c.KnowledgeId).ToList()
            };
            
            var assistantMessage = new ChatMessage
            {
                ConversationId = finalConversationId,
                Content = jsonString, // Lưu JSON string thay vì text thuần
                Role = "assistant",
                CreatedAt = DateTime.UtcNow,
                IsRelevant = true,
                Metadata = JsonSerializer.Serialize(metadata, _jsonOptions)
            };
            await _messageRepository.AddAsync(assistantMessage);
            Console.WriteLine($"Saved assistant response to conversation {finalConversationId}");
            
            // Lưu Q&A vào knowledge base để học từ các cuộc trò chuyện
            _ = Task.Run(async () =>
            {
                try
                {
                    await SaveConversationToKnowledgeAsync(message, fullResponse, finalConversationId, userId);
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Error saving conversation to knowledge base: {ex.Message}");
                    // Không throw - không block response
                }
            });
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error saving assistant response: {ex.Message}");
            Console.WriteLine($"Stack trace: {ex.StackTrace}");
            // Don't throw - response already sent to client
        }
    }

    /// <summary>
    /// Lưu Q&A từ cuộc trò chuyện vào knowledge base để hệ thống có thể học và tự trả lời các câu hỏi tương tự sau này
    /// </summary>
    private async Task SaveConversationToKnowledgeAsync(
        string userQuestion,
        string assistantAnswer,
        int conversationId,
        string userId)
    {
        try
        {
            // Chỉ lưu nếu câu trả lời có ý nghĩa (không quá ngắn, không phải lỗi)
            if (string.IsNullOrWhiteSpace(assistantAnswer) || 
                assistantAnswer.Length < 20 ||
                assistantAnswer.Contains("Xin lỗi, tôi chỉ có thể") ||
                assistantAnswer.Contains("không thể"))
            {
                Console.WriteLine($"[SaveConversationToKnowledge] Skipping - answer too short or error message");
                return;
            }

            // Tạo nội dung Q&A để lưu vào knowledge base
            // Format: "Câu hỏi: {question}\n\nTrả lời: {answer}"
            var qaContent = $"Câu hỏi: {userQuestion}\n\nTrả lời: {assistantAnswer}";
            
            // Giới hạn độ dài để tránh quá dài
            if (qaContent.Length > 4000)
            {
                qaContent = qaContent.Substring(0, 4000) + "...";
            }

            Console.WriteLine($"[SaveConversationToKnowledge] Saving Q&A for conversation {conversationId}");

            // Tạo embedding cho Q&A content
            var embedding = await _embeddingsService.EmbedData(qaContent);
            var embeddingJson = CommonUtils.ObjectToString(embedding);

            // Tạo metadata
            var metadata = new
            {
                conversationId = conversationId,
                userId = userId,
                question = userQuestion,
                answer = assistantAnswer.Length > 500 ? assistantAnswer.Substring(0, 500) + "..." : assistantAnswer,
                createdAt = DateTime.UtcNow
            };
            var metadataJson = JsonSerializer.Serialize(metadata, _jsonOptions);

            // Lưu vào AssistantKnowledge
            var knowledge = new AssistantKnowledge
            {
                Content = qaContent,
                EmbeddingData = embeddingJson,
                SourceType = "ChatConversation",
                SourceId = conversationId,
                Metadata = metadataJson,
                CreatedAt = DateTime.UtcNow
            };

            await _knowledgeRepository.AddAsync(knowledge);
            Console.WriteLine($"[SaveConversationToKnowledge] Successfully saved knowledge ID: {knowledge.KnowledgeId}");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[SaveConversationToKnowledge] Error: {ex.Message}");
            Console.WriteLine($"Stack trace: {ex.StackTrace}");
            // Không throw - không block flow chính
        }
    }

    public async Task<string> CreateConversationAsync(string userId, string? initialMessage = null)
    {
        var conversation = new ChatConversation
        {
            UserId = userId,
            Title = initialMessage != null && initialMessage.Length > 50 
                ? initialMessage.Substring(0, 50) + "..." 
                : initialMessage ?? "Cuộc trò chuyện mới",
            PersonaType = null, // Will be set on first message
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow,
            IsActive = true
        };

        conversation = await _conversationRepository.AddAsync(conversation);
        return conversation.ConversationId.ToString();
    }

    public async Task<List<ChatMessage>> GetConversationHistoryAsync(string conversationId, int limit = 20)
    {
        if (int.TryParse(conversationId, out var convId))
        {
            var messageList = await _messageRepository.GetByConversationIdAsync(convId, limit);
            foreach(var chatMessage in messageList)
            {
                var content = chatMessage.Content;
                if (string.IsNullOrWhiteSpace(content))
                    continue;

                content = content.Trim();

                try
                {
                    if (content.StartsWith("{"))
                    {
                        var obj = JsonSerializer.Deserialize<ChatResponse>(content, _jsonOptions);
                        if (obj != null && obj.Text != null)
                        {
                            // Lấy text từ JSON object (Unicode đã được decode tự động)
                            content = obj.Text;
                        }
                    }
                }
                catch
                {
                    // Parse lỗi → giữ nguyên content gốc
                }
                chatMessage.Content = content;
            }
            return messageList;
        }
        return new List<ChatMessage>();
    }

    public class ChatResponse
    {
        public string Text { get; set; }
        public string SystemData { get; set; }
    }

}

