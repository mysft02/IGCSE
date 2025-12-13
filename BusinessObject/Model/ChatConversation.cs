using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace BusinessObject.Model;

[Table("chatconversation")]
public class ChatConversation
{
    [Key]
    [Column("ConversationId")]
    public int ConversationId { get; set; }

    [Required]
    [Column("UserId")]
    [StringLength(255)]
    public string UserId { get; set; } = null!;

    [Column("Title")]
    [StringLength(500)]
    public string? Title { get; set; }

    [Column("PersonaType")]
    [StringLength(50)]
    public string? PersonaType { get; set; }

    [Required]
    [Column("CreatedAt")]
    public DateTime CreatedAt { get; set; }

    [Required]
    [Column("UpdatedAt")]
    public DateTime UpdatedAt { get; set; }

    [Required]
    [Column("IsActive")]
    public bool IsActive { get; set; } = true;

    // Navigation properties
    public virtual Account? User { get; set; }
    public virtual ICollection<ChatMessage> Messages { get; set; } = new List<ChatMessage>();
}

