using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace BusinessObject.Model
{
    [Table("course")]
    public class Course
    {
        [Key]
        [Column("id")]
        public long Id { get; set; }

        [Required]
        [Column("name")]
        public string Name { get; set; } = string.Empty;

        [Column("description")]
        public string? Description { get; set; }

        [Required]
        [Column("status")]
        public string Status { get; set; } = string.Empty;

        [Column("category")]
        public string? Category { get; set; }

        [Column("createdat")]
        public DateTime? CreatedAt { get; set; }

        [Column("updatedat")]
        public DateTime? UpdatedAt { get; set; }

        [Column("createdby")]
        [ForeignKey("AspNetUsers")]
        public string? CreatedBy { get; set; }

        [Column("updatedby")]
        public string? UpdatedBy { get; set; }
    }
}


