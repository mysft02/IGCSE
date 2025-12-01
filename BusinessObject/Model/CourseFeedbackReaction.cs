using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace BusinessObject.Model;

public class CourseFeedbackReaction
{
    [Key]
    public int CourseFeedbackReactionId { get; set; }

    public int CourseFeedbackId { get; set; }

    [Required]
    [MaxLength(255)]
    public string UserId { get; set; } = null!;

    [Required]
    [MaxLength(10)]
    public string ReactionType { get; set; } = null!; // "Like" or "Unlike"

    public DateTime CreatedAt { get; set; }

    public DateTime? UpdatedAt { get; set; }

    [ForeignKey(nameof(CourseFeedbackId))]
    public virtual Coursefeedback CourseFeedback { get; set; } = null!;

    [ForeignKey(nameof(UserId))]
    public virtual Account User { get; set; } = null!;
}

