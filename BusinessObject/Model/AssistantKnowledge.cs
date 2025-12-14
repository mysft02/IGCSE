using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace BusinessObject.Model;

[Table("assistantknowledge")]
public class AssistantKnowledge
{
    [Key]
    [Column("KnowledgeId")]
    public int KnowledgeId { get; set; }

    [Required]
    [Column("Content")]
    public string Content { get; set; } = null!;

    [Required]
    [Column("EmbeddingData")]
    public string EmbeddingData { get; set; } = null!; // JSON array of floats

    [Column("SourceType")]
    [StringLength(50)]
    public string? SourceType { get; set; } // "Course", "Section", "Lesson"

    [Column("SourceId")]
    public int? SourceId { get; set; }

    [Column("Metadata")]
    public string? Metadata { get; set; } // JSON object

    [Required]
    [Column("CreatedAt")]
    public DateTime CreatedAt { get; set; }
}

