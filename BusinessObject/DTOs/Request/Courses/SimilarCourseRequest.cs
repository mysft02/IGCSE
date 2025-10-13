using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BusinessObject.DTOs.Request.Courses
{
    public class SimilarCourseRequest
    {
        public long CourseId { get; set; }
        public decimal Score { get; set; }
    }
}
