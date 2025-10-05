using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BusinessObject.Model
{
    public class Course
    {
        public int CourseID { get; set; }
        public string CourseName { get; set; }
        public string Description { get; set; }
        public CourseStatus Status { get; set; } = CourseStatus.Draft;
        public decimal Price { get; set; }
        public string ImageUrl { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
        public int CategoryID { get; set; }
        public Category? Category { get; set; }
    }

    public enum CourseStatus
    {
        Draft = 0,
        Publish = 1,
        Unpublish = 2
    }
}
