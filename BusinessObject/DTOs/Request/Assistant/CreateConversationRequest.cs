using System.ComponentModel.DataAnnotations;
using Swashbuckle.AspNetCore.Annotations;

namespace BusinessObject.DTOs.Request.Assistant;

public class CreateConversationRequest
{
    [StringLength(500)]
    [SwaggerSchema("Tin nhắn đầu tiên để tạo cuộc trò chuyện", Nullable = true)]
    public string? InitialMessage { get; set; }
}

