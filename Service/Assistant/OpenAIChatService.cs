using BusinessObject.Payload.Request.OpenAI;
using Service.OpenAI;
using System.Text.Json;
using System.Text;

namespace Service.Assistant;

public class OpenAIChatService
{
    private readonly OpenAIApiService _openAIApiService;
    private const string ChatEndpoint = "/chat/completions";

    public OpenAIChatService(OpenAIApiService openAIApiService)
    {
        _openAIApiService = openAIApiService;
    }

    public async Task<string> ChatAsync(string systemPrompt, string userMessage, List<string>? conversationHistory = null)
    {
        var messages = new List<OpenAIChatMessage>
        {
            new() { Role = "system", Content = systemPrompt }
        };

        // Add conversation history
        if (conversationHistory != null && conversationHistory.Any())
        {
            foreach (var history in conversationHistory)
            {
                // Assume alternating user/assistant messages
                // This is simplified - in production, parse properly
            }
        }

        messages.Add(new OpenAIChatMessage { Role = "user", Content = userMessage });

        var body = new OpenAIChatBody
        {
            Model = "gpt-4o-mini",
            Messages = messages,
            Temperature = 0.7,
            MaxTokens = 1000,
            Stream = false
        };

        var request = BusinessObject.Payload.Request.OpenAI.OpenApiRequest.Builder()
            .CallUrl(ChatEndpoint)
            .Body(body)
            .Build();

        var response = await _openAIApiService.PostAsync<OpenAIChatBody, OpenAIChatResponse>(request, body);

        return response?.Choices?.FirstOrDefault()?.Message?.Content ?? "Xin lỗi, tôi không thể trả lời câu hỏi này.";
    }

    public async IAsyncEnumerable<string> ChatStreamAsync(string systemPrompt, string userMessage, List<string>? conversationHistory = null)
    {
        var messages = new List<OpenAIChatMessage>
        {
            new() { Role = "system", Content = systemPrompt }
        };

        messages.Add(new OpenAIChatMessage { Role = "user", Content = userMessage });

        var body = new OpenAIChatBody
        {
            Model = "gpt-4o-mini",
            Messages = messages,
            Temperature = 0.7,
            MaxTokens = 1000,
            Stream = true
        };

        // For streaming, we need to use HTTP client directly
        // This is a simplified version - in production, implement proper streaming
        var fullResponse = await ChatAsync(systemPrompt, userMessage, conversationHistory);
        
        // Simulate streaming by chunking the response
        var words = fullResponse.Split(' ');
        foreach (var word in words)
        {
            yield return word + " ";
            await Task.Delay(50); // Simulate network delay
        }
    }
}

