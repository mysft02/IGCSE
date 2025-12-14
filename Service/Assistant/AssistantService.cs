using BusinessObject.DTOs.Response.Assistant;
using BusinessObject.Model;
using Repository.IRepositories;
using System.Text.Json;

namespace Service.Assistant;

public class AssistantService : IAssistantService
{
    private readonly IContextManager _contextManager;
    private readonly IPersonaService _personaService;
    private readonly IRAGService _ragService;
    private readonly OpenAIChatService _openAIChatService;
    private readonly IChatMessageRepository _messageRepository;
    private readonly IChatConversationRepository _conversationRepository;

    public AssistantService(
        IContextManager contextManager,
        IPersonaService personaService,
        IRAGService ragService,
        OpenAIChatService openAIChatService,
        IChatMessageRepository messageRepository,
        IChatConversationRepository conversationRepository)
    {
        _contextManager = contextManager;
        _personaService = personaService;
        _ragService = ragService;
        _openAIChatService = openAIChatService;
        _messageRepository = messageRepository;
        _conversationRepository = conversationRepository;
    }

    public async Task<AssistantResponse> ProcessMessageAsync(
        string userId,
        string message,
        string? conversationId = null)
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

        // 2. Build context
        var context = await _contextManager.BuildContextAsync(userId);

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

        // 7. Call OpenAI
        var response = await _openAIChatService.ChatAsync(systemPrompt, message, history);

        // 8. Save messages
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

        // Save assistant response
        var assistantMessage = new ChatMessage
        {
            ConversationId = finalConversationId,
            Content = response,
            Role = "assistant",
            CreatedAt = DateTime.UtcNow,
            IsRelevant = true,
            Metadata = JsonSerializer.Serialize(new { RelevantKnowledgeIds = relevantContexts.Select(c => c.KnowledgeId).ToList() })
        };
        await _messageRepository.AddAsync(assistantMessage);

        return new AssistantResponse
        {
            Content = response,
            IsRelevant = true,
            ConversationId = finalConversationId.ToString(),
            RelevantKnowledgeIds = relevantContexts.Select(c => c.KnowledgeId).ToList()
        };
    }

    public async IAsyncEnumerable<string> ProcessMessageStreamAsync(
        string userId,
        string message,
        string? conversationId = null)
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

        var context = await _contextManager.BuildContextAsync(userId);
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

        // Save assistant response - wrap in try-catch for error handling
        try
        {
            var assistantMessage = new ChatMessage
            {
                ConversationId = finalConversationId,
                Content = fullResponse,
                Role = "assistant",
                CreatedAt = DateTime.UtcNow,
                IsRelevant = true,
                Metadata = JsonSerializer.Serialize(new { RelevantKnowledgeIds = relevantContexts.Select(c => c.KnowledgeId).ToList() })
            };
            await _messageRepository.AddAsync(assistantMessage);
            Console.WriteLine($"Saved assistant response to conversation {finalConversationId}");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error saving assistant response: {ex.Message}");
            Console.WriteLine($"Stack trace: {ex.StackTrace}");
            // Don't throw - response already sent to client
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
            return await _messageRepository.GetByConversationIdAsync(convId, limit);
        }
        return new List<ChatMessage>();
    }
}

