using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace BusinessObject.Model;

[Table("chatmessage")]
public class ChatMessage
{
    [Key]
    [Column("MessageId")]
    public int MessageId { get; set; }

    [Required]
    [Column("ConversationId")]
    public int ConversationId { get; set; }

    [Required]
    [Column("Content")]
    public string Content { get; set; } = null!;

    [Required]
    [Column("Role")]
    [StringLength(20)]
    public string Role { get; set; } = null!; // "user" or "assistant"

    [Required]
    [Column("CreatedAt")]
    public DateTime CreatedAt { get; set; }

    [Column("Metadata")]
    public string? Metadata { get; set; }

    [Required]
    [Column("IsRelevant")]
    public bool IsRelevant { get; set; } = true;

    // Navigation properties
    [ForeignKey("ConversationId")]
    public virtual ChatConversation? Conversation { get; set; }
}

