using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;

namespace BusinessObject.Model;

public partial class Course
{
    public int CourseId { get; set; }

    public string Name { get; set; } = null!;

        [Column("description")]
        public string? Description { get; set; }

    public string Status { get; set; } = null!;

    public decimal? Price { get; set; }

    public string? ImageUrl { get; set; }

    public int? CategoryId { get; set; }

        public DateTime? CreatedAt { get; set; }

        [Column("updatedat")]
        public DateTime? UpdatedAt { get; set; }

        [Column("createdby")]
        [ForeignKey("AspNetUsers")]
        public string? CreatedBy { get; set; }

        [Column("updatedby")]
        public string? UpdatedBy { get; set; }

    public virtual Category? Category { get; set; }
}


