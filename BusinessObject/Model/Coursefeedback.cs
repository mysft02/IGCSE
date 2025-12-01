using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace BusinessObject.Model;

public class Coursefeedback
{
    [Key]
    public int CourseFeedbackId { get; set; }

    public int CourseId { get; set; }

    [Required]
    [MaxLength(255)]
    public string StudentId { get; set; } = null!;

    [Range(1, 5)]
    public int Rating { get; set; }

    public string? Comment { get; set; }

    public DateTime CreatedAt { get; set; }

    public DateTime? UpdatedAt { get; set; }

    public int LikeCount { get; set; } = 0;

    public int UnlikeCount { get; set; } = 0;

    [ForeignKey(nameof(CourseId))]
    public virtual Course Course { get; set; } = null!;

    [ForeignKey(nameof(StudentId))]
    public virtual Account Student { get; set; } = null!;

    public virtual ICollection<CourseFeedbackReaction> Reactions { get; set; } = new List<CourseFeedbackReaction>();
}

