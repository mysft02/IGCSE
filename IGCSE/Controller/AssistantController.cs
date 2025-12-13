using BusinessObject.DTOs.Request.Assistant;
using BusinessObject.DTOs.Response;
using BusinessObject.DTOs.Response.Assistant;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Service.Assistant;
using Swashbuckle.AspNetCore.Annotations;
using System.Security.Claims;

namespace IGCSE.Controller;

[Route("api/assistant")]
[ApiController]
[Authorize]
public class AssistantController : ControllerBase
{
    private readonly IAssistantService _assistantService;

    public AssistantController(IAssistantService assistantService)
    {
        _assistantService = assistantService;
    }

    /// <summary>
    /// Lấy danh sách cuộc trò chuyện của user
    /// </summary>
    [HttpGet("conversations")]
    [SwaggerOperation(
        Summary = "Lấy danh sách cuộc trò chuyện",
        Description = "Lấy tất cả cuộc trò chuyện của user hiện tại"
    )]
    [SwaggerResponse(200, "Thành công", typeof(BaseResponse<List<ConversationResponse>>))]
    public async Task<IActionResult> GetConversations(
        [FromServices] Repository.IRepositories.IChatConversationRepository conversationRepository)
    {
        var userId = User.FindFirst("AccountID")?.Value;
        if (string.IsNullOrEmpty(userId))
        {
            return Unauthorized(BaseResponse<List<ConversationResponse>>.Error("Unauthorized"));
        }

        var conversations = await conversationRepository.GetByUserIdAsync(userId);
        var response = conversations.Select(c => new ConversationResponse
        {
            ConversationId = c.ConversationId,
            Title = c.Title,
            PersonaType = c.PersonaType,
            CreatedAt = c.CreatedAt,
            UpdatedAt = c.UpdatedAt,
            IsActive = c.IsActive
        }).ToList();

        return Ok(BaseResponse<List<ConversationResponse>>.Success(response));
    }

    /// <summary>
    /// Lấy lịch sử tin nhắn của cuộc trò chuyện
    /// </summary>
    [HttpGet("conversations/{conversationId}/messages")]
    [SwaggerOperation(
        Summary = "Lấy lịch sử tin nhắn",
        Description = "Lấy lịch sử tin nhắn của cuộc trò chuyện"
    )]
    [SwaggerResponse(200, "Thành công", typeof(BaseResponse<List<MessageResponse>>))]
    public async Task<IActionResult> GetMessages(
        int conversationId,
        [FromServices] Repository.IRepositories.IChatMessageRepository messageRepository,
        [FromQuery] int limit = 20)
    {
        var messages = await _assistantService.GetConversationHistoryAsync(conversationId.ToString(), limit);
        var response = messages.Select(m => new MessageResponse
        {
            MessageId = m.MessageId,
            Content = m.Content,
            Role = m.Role,
            CreatedAt = m.CreatedAt,
            Metadata = m.Metadata,
            IsRelevant = m.IsRelevant
        }).ToList();

        return Ok(BaseResponse<List<MessageResponse>>.Success(response));
    }

    /// <summary>
    /// Tạo cuộc trò chuyện mới
    /// </summary>
    [HttpPost("conversations")]
    [SwaggerOperation(
        Summary = "Tạo cuộc trò chuyện mới",
        Description = "Tạo một cuộc trò chuyện mới với trợ lý AI"
    )]
    [SwaggerResponse(201, "Tạo thành công", typeof(BaseResponse<ConversationResponse>))]
    public async Task<IActionResult> CreateConversation(
        [FromBody] CreateConversationRequest request,
        [FromServices] Repository.IRepositories.IChatConversationRepository conversationRepository)
    {
        var userId = User.FindFirst("AccountID")?.Value;
        if (string.IsNullOrEmpty(userId))
        {
            return Unauthorized(BaseResponse<ConversationResponse>.Error("Unauthorized"));
        }

        var conversationId = await _assistantService.CreateConversationAsync(userId, request.InitialMessage);
        
        var conversation = await conversationRepository.GetByIdAsync(int.Parse(conversationId));
        if (conversation == null)
        {
            return NotFound(BaseResponse<ConversationResponse>.Error("Không tìm thấy cuộc trò chuyện"));
        }

        var response = new ConversationResponse
        {
            ConversationId = conversation.ConversationId,
            Title = conversation.Title,
            PersonaType = conversation.PersonaType,
            CreatedAt = conversation.CreatedAt,
            UpdatedAt = conversation.UpdatedAt,
            IsActive = conversation.IsActive
        };

        return Created($"/api/assistant/conversations/{conversationId}", 
            BaseResponse<ConversationResponse>.Success(response));
    }

    /// <summary>
    /// Xóa cuộc trò chuyện
    /// </summary>
    [HttpDelete("conversations/{conversationId}")]
    [SwaggerOperation(
        Summary = "Xóa cuộc trò chuyện",
        Description = "Xóa một cuộc trò chuyện (soft delete - set IsActive = false)"
    )]
    [SwaggerResponse(200, "Xóa thành công", typeof(BaseResponse<string>))]
    public async Task<IActionResult> DeleteConversation(
        int conversationId,
        [FromServices] Repository.IRepositories.IChatConversationRepository conversationRepository)
    {
        var userId = User.FindFirst("AccountID")?.Value;
        if (string.IsNullOrEmpty(userId))
        {
            return Unauthorized(BaseResponse<string>.Error("Unauthorized"));
        }

        var conversation = await conversationRepository.GetByIdAsync(conversationId);
        if (conversation == null || conversation.UserId != userId)
        {
            return NotFound(BaseResponse<string>.Error("Không tìm thấy cuộc trò chuyện"));
        }

        conversation.IsActive = false;
        conversation.UpdatedAt = DateTime.UtcNow;
        await conversationRepository.UpdateAsync(conversation);

        return Ok(BaseResponse<string>.Success("Cuộc trò chuyện đã được xóa"));
    }

    /// <summary>
    /// Gửi tin nhắn (REST API - không streaming)
    /// </summary>
    [HttpPost("conversations/{conversationId}/messages")]
    [SwaggerOperation(
        Summary = "Gửi tin nhắn",
        Description = "Gửi tin nhắn đến trợ lý AI (REST API, không streaming). Để có streaming, sử dụng SignalR Hub."
    )]
    [SwaggerResponse(200, "Thành công", typeof(BaseResponse<AssistantResponse>))]
    public async Task<IActionResult> SendMessage(
        int conversationId,
        [FromBody] string message)
    {
        var userId = User.FindFirst("AccountID")?.Value;
        if (string.IsNullOrEmpty(userId))
        {
            return Unauthorized(BaseResponse<AssistantResponse>.Error("Unauthorized"));
        }

        // Get user role from JWT token
        var userRole = User.FindFirst(ClaimTypes.Role)?.Value;

        var response = await _assistantService.ProcessMessageAsync(userId, message, conversationId.ToString(), userRole);
        return Ok(BaseResponse<AssistantResponse>.Success(response));
    }
}

