using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;
using BusinessObject.Enums;

namespace BusinessObject.Model
{
    [Table("module")]
    public class Module
    {
        [Key]
        [Column("ModuleID")]
        public int ModuleID { get; set; }

        [Column("ModuleName")]
        [Required]
        [StringLength(255)]
        public string ModuleName { get; set; } = null!;

        [Column("Description")]
        public string? Description { get; set; }

        [Column("IsActive")]
        public bool IsActive { get; set; } = true;

        [Column("CreatedAt")]
        public DateTime? CreatedAt { get; set; } = DateTime.UtcNow;

        [Column("UpdatedAt")]
        public DateTime? UpdatedAt { get; set; } = DateTime.UtcNow;

        [Column("EmbeddingDataSubject")]
        public string? EmbeddingDataSubject { get; set; }

        [NotMapped]
        public CourseSubject CourseSubject
        {
            get => string.IsNullOrEmpty(EmbeddingDataSubject) ? default : 
                   Enum.TryParse<CourseSubject>(EmbeddingDataSubject, out var result) ? result : default;
            set => EmbeddingDataSubject = value.ToString();
        }
        
        [JsonIgnore]
        public virtual ICollection<Course> Courses { get; set; } = new List<Course>();
    }
}
