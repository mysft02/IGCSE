using System.Text.Json.Serialization;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace BusinessObject.Model;

public class Course
{
    public int CourseId { get; set; }

    public string Name { get; set; } = null!;

    public string? Description { get; set; }

    public string Status { get; set; } = null!;

    public int? ModuleId { get; set; }

    public DateTime? CreatedAt { get; set; }

    public DateTime? UpdatedAt { get; set; }

    public string? CreatedBy { get; set; }

    public string? UpdatedBy { get; set; }

    public decimal Price { get; set; }

    public string? ImageUrl { get; set; }

    [JsonIgnore]
    public string EmbeddingData { get; set; } = null!;

    [ForeignKey("ModuleId")]
    public virtual Module? Module { get; set; }

    public virtual Finalquiz FinalQuiz { get; set; }

    public virtual ICollection<Coursesection> CourseSections { get; set; }
}
